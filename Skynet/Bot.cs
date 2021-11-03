namespace Skynet;

using DisCatSharp;
using DisCatSharp.EventArgs;
using DisCatSharp.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Discord;
using Interfaces;
using Options;
using DisCatSharp.Entities;
using System.Reflection;
using Skynet.Discord.Attributes;
using Skynet.QuoteService.Interfaces;
using Skynet.Services;
using Skynet.ImageCreator.Utilities;
using Skynet.ImageCreator.Interfaces;
using Skynet.Extensions;

public class Bot : DiscordHostedService
{
    private readonly IWelcomeHandler _welcomeHandler;
    private readonly DiscordOptions _options;
    private readonly Dictionary<string, IInteractionHandler> _interactionHandlers = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly Images _imageUtility = new();
    private readonly IImageService _imageService;
    private readonly ImageSearchOptions _searchOptions;

    public Bot(IConfiguration config, ILogger<Bot> logger, 
        DiscordOptions options,
        ImageSearchOptions searchOptions,
        IImageService imageService,
        IServiceProvider provider, IHostApplicationLifetime lifetime) : base(config, logger, provider, lifetime)
    {
        _serviceProvider = provider;
        _imageService = imageService;
        _welcomeHandler = provider.GetRequiredService<IWelcomeHandler>();
        _options = options;
        _searchOptions = searchOptions;

        Client.ComponentInteractionCreated += ClientOnComponentInteractionCreated;
        Client.GuildMemberAdded += Client_GuildMemberAdded;
        Client.MessageCreated += Client_MessageCreated;
    }

    private async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot)
            return;
        
        if(e.Message.Content.StartsWith("test-quotes"))
        {
            var quotes = _serviceProvider.GetRequiredService<QuoteHandler>();
            await quotes.SendQuote();
        }
        else if(e.Message.Content.StartsWith("test-image"))
        {
            var member = await e.Guild.GetMemberAsync(e.Author.Id);
            var url = await _imageService.RandomImageUrl(_searchOptions.WelcomeKeywords.RandomItem());
            var image = await _imageUtility.CreateImageAsync(new(member.AvatarUrl, member.Username, "Welcome!!"), url);
            using var file = File.Open(image, FileMode.Open);
            await e.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithFile(file));
            file.Close();
        }
    }

    protected override void OnInitializationError(Exception ex)
    {
        Logger.LogCritical($"{ex.Message} bad things happened");
        base.OnInitializationError(ex);
    }

    internal DiscordGuild MainGuild => Client.Guilds[_options.MainGuildId];
    internal DiscordChannel GeneralChannel => MainGuild.Channels[_options.GeneralChannelId];

    private async Task Client_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        var guild = await Client.GetGuildAsync(_options.MainGuildId);
        var channel = guild.GetChannel(_options.GeneralChannelId);
        string? image= null;
        
        try
        {
            var url = await _imageService.RandomImageUrl(_searchOptions.WelcomeKeywords.RandomItem());
            image = await _imageUtility.CreateImageAsync(new(e.Member.AvatarUrl, e.Member.Username, "Welcome!!"), url);
            using var file = File.Open(image, FileMode.Open);
            await channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithFile(file));
            file.Close();
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, $"Was unable to properly welcome {e.Member.Username}");
        }
        finally
        {
            if(!string.IsNullOrEmpty(image))
                File.Delete(image);
        }        
    }

    /// <summary>
    /// Create an instance of each interaction handler within this assembly
    /// </summary>
    void InitializeInteractionHandlers()
    {
        var types = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(x => x.IsAssignableTo(typeof(IInteractionHandler)) && !x.IsInterface && !x.IsAbstract)
            .ToList();

        foreach(var type in types)
        {
            var instance = (IInteractionHandler)ActivatorUtilities.CreateInstance(_serviceProvider, type);

            var attributes = type.GetCustomAttributes<InteractionNameAttribute>();

            if(!attributes.Any())
            {
                Logger.LogWarning($"Found an interaction handler {type.Name} -- but it doesn't have the {nameof(InteractionNameAttribute)} attribute");
                continue;
            }

            foreach (var attribute in attributes)
                _interactionHandlers.Add(attribute.Name, instance);
        }
    }

    async Task ClientOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        Logger.LogInformation($"Interaction ID: {e.Id} : {e.Message.Content} : {string.Join(", ", e.Values)}");
        var member = await e.Guild.GetMemberAsync(e.User.Id);

        if (!_interactionHandlers.Any())
            InitializeInteractionHandlers();

        // if a role based interaction was made on the welcome channel
        if(e.Id.EndsWith(InteractionConstants.LECTURE_JOIN_ROLE) && e.Channel.Id == _options.WelcomeChannelId)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                 .WithTitle("Sorting Hat")
                 .WithDescription("Role successfully applied");

            DiscordInteractionResponseBuilder interactionBuilder = new DiscordInteractionResponseBuilder()
            {
                IsEphemeral = true
            };

            interactionBuilder.AddEmbed(embedBuilder.Build());
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, interactionBuilder);
            await _welcomeHandler.AddRoleToMember(member, e.Id);
            return;
        }

        var interactionId = e.Id.Split('_').Last();

        if(_interactionHandlers.ContainsKey(interactionId))
        {
            Logger.LogInformation($"Handling interaction id: {interactionId}");
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await _interactionHandlers[interactionId].Handle(member, e);
        }

        if(_interactionHandlers.ContainsKey(e.Id))
        {
            Logger.LogInformation($"Handling interaction id: {e.Id}");

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await _interactionHandlers[e.Id].Handle(member, e);
        }
    }
}
