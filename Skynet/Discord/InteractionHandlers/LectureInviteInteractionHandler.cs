namespace Skynet.Discord.InteractionHandlers;
using Attributes;
using Options;
using Interfaces;
using DisCatSharp.Hosting;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

[InteractionName(InteractionConstants.LECTURE_JOIN_ROLE)]
public class LectureInviteInteractionHandler : IInteractionHandler
{
    private readonly DiscordOptions _discordOptions;
    private readonly WelcomeOptions _welcomeOptions;
    private readonly IWelcomeHandler _welcomeHandler;
    private readonly IDiscordHostedService _bot;

    public LectureInviteInteractionHandler(DiscordOptions discordOptions, WelcomeOptions welcomeOptions,
        IWelcomeHandler welcomeHandler,
        IDiscordHostedService bot)
    {
        _discordOptions = discordOptions;
        _welcomeOptions = welcomeOptions;
        _bot= bot;
        _welcomeHandler = welcomeHandler;
    }

    public async Task<bool> Handle(DiscordMember member, ComponentInteractionCreateEventArgs args)
    {
        DiscordFollowupMessageBuilder responseBuilder = new()
        {
            IsEphemeral = true
        };

        var interaction = args.Interaction;
        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Lecture Assistant")
            .WithDescription($"Anyone who joins within the next " +
            $"{_welcomeOptions.InviteWelcomeDuration} hours will be shown a button to quickly join your selected roles")
            .WithImageUrl(_discordOptions.InviteImage)
            .WithFooter(_discordOptions.InviteFooter)
            .WithColor(DiscordColor.Green);

        responseBuilder.AddEmbed(embedBuilder.Build());

        await interaction.CreateFollowupMessageAsync(responseBuilder);
        await interaction.DeleteOriginalResponseAsync();
        DateTime expirationTime = DateTime.Now.AddHours(_welcomeOptions.InviteWelcomeDuration);

        var guild = _bot.Client.Guilds[_discordOptions.MainGuildId];

        foreach (var entry in args.Values)
            _welcomeHandler.AddClass(guild.Roles[ulong.Parse(entry)], expirationTime);

        args.Handled = true;
        return true;
    }

}
