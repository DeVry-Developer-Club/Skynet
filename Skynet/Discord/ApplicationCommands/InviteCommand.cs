using DisCatSharp.ApplicationCommands;
namespace Skynet.Discord.ApplicationCommands;

using DisCatSharp.Entities;
using Options;
using Skynet.Discord.Extensions;
using Interfaces;
using DisCatSharp.Interactivity;
using DisCatSharp.EventArgs;

public class InviteCommand : ApplicationCommandsModule
{
    public DiscordOptions DiscordOptions { get; set; }
    public IWelcomeHandler WelcomeHandler { get; set; }
    public ILogger<InviteCommand> Logger { get; set; }
    public WelcomeOptions WelcomeOptions { get; set; }
    public IRoleService RoleService { get; set; }
    public DisCatSharp.Hosting.IDiscordHostedService Bot { get; set; }


    [SlashCommand("invite", "Display the permanent invite for the server")]
    public async Task Command(InteractionContext context)
    {
        if (!await context.ValidateGuild())
            return;

        DiscordInteractionResponseBuilder responseBuilder = new();

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Invitation")
            .WithAuthor("Recruiting Hat")
            .WithDescription(DiscordOptions.InviteMessage)
            .AddField("Invite", DiscordOptions.InviteLink)
            .WithFooter(DiscordOptions.InviteFooter)
            .WithImageUrl(DiscordOptions.InviteImage);

        responseBuilder.AddEmbed(embedBuilder.Build());

        await context.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, responseBuilder);
    }

    [SlashCommand("lecture-invite", "Help your fellow classmates")]
    public async Task LectureCommand(InteractionContext context)
    {
        if (!await context.ValidateGuild())
            return;

        await context.ImThinking();

        DiscordWebhookBuilder responseBuilder = new();
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Sorting Hat")
            .WithDescription(DiscordOptions.InviteMessage)
            .WithFooter(DiscordOptions.InviteFooter)
            .WithImageUrl(DiscordOptions.InviteImage);

        responseBuilder.AddEmbed(embedBuilder.Build());

        string menuId = $"{context.Member.Id}_{InteractionConstants.LECTURE_INVITE}";

        // allow users to select from one of their roles -- removing anything that's blacklisted
        var roles = context.Member.Roles.RemoveBlacklistedRoles(RoleService.GetBlacklistedRolesDict(DiscordOptions.MainGuildId).Keys)
            .OrderBy(x => x.Name)
            .Take(24)
            .ToList();

        if(!roles.Any())
        {
            DiscordFollowupMessageBuilder messageBuilder = new()
            {
                IsEphemeral = true
            };

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle("Oops")
                .WithDescription("Sorry, this command assumes you have roles to choose from!")
                .WithColor(DiscordColor.Yellow);

            messageBuilder.AddEmbed(builder.Build());
            await context.FollowUpAsync(messageBuilder);
        }

        List<DiscordSelectComponentOption> options = new();

        foreach (var role in roles)
            options.Add(new DiscordSelectComponentOption(role.Name, role.Id.ToString()));

        DiscordSelectComponent menu = new DiscordSelectComponent(menuId, "Invite is for", options, 1, options.Count, false);

        responseBuilder.AddComponents(menu);
        var message = await context.EditResponseAsync(responseBuilder);

        embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Lecture Assistant")
            .WithDescription("Anyone who joins within the next " +
            $"{WelcomeOptions.InviteWelcomeDuration} hours will be shown a button to quickly join your selected roles")
            .WithImageUrl(DiscordOptions.InviteImage)
            .WithFooter(DiscordOptions.InviteFooter)
            .WithColor(DiscordColor.Green);

        var interactivity = Bot.Client.GetExtension<InteractivityExtension>();
        InteractivityResult<ComponentInteractionCreateEventArgs> interaction = await interactivity.WaitForSelectAsync(message, menuId, timeoutOverride: null);
        responseBuilder = new();
        responseBuilder.AddEmbed(embedBuilder.Build());
        await context.EditResponseAsync(responseBuilder);

        DateTime expirationTime = DateTime.Now.AddHours(WelcomeOptions.InviteWelcomeDuration);
        foreach (var entry in interaction.Result.Values)
            WelcomeHandler.AddClass(context.Guild.Roles[ulong.Parse(entry)], expirationTime);
    }
}
