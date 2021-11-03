namespace Skynet.Discord.Extensions;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Hosting;
using DisCatSharp.Interactivity;

public static class InteractionExtensions
{
    /// <summary>
    /// Ensure the interaction context contains a valid guild
    /// Ran on application commands
    /// </summary>
    /// <param name="context"></param>
    /// <returns>True if valid, otherwise false</returns>
    public static async Task<bool> ValidateGuild(this InteractionContext context)
    {
        if (context.Guild != null)
            return true;

        DiscordInteractionResponseBuilder discordInteractionResponseBuilder = new()
        {
            Content = "Error: This is a guild command!",
            IsEphemeral = true
        };

        await context.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource,
            discordInteractionResponseBuilder);

        return false;
    }

    /// <summary>
    /// Send user a timeout message because they took too long to response
    /// </summary>
    /// <param name="context"></param>
    /// <param name="message"></param>   
    public static async Task SendTimeout(this InteractionContext context, string message)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("You took too long")
            .WithDescription(message)
            .WithTimestamp(DateTime.Now);

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
    }

    /// <summary>
    /// Retrieve a boolean value from the user
    /// </summary>
    /// <param name="context"></param>
    /// <param name="message">Prompt for user to answer yes/no to</param>
    /// <param name="bot"></param>
    /// <returns>True if yes, otherwise false</returns>
    public static async Task<bool> YesNo(this InteractionContext context, string message, IDiscordHostedService bot)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Require Input")
            .WithDescription(message)
            .WithColor(DiscordColor.Cyan)
            .WithFooter("Please click a button below");

        string yesId = $"{context.Member.Id}_yes";
        string noId = $"{context.Member.Id}_no";

        DiscordButtonComponent yesButton = 
            new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Success, yesId, "Yes", false, null);

        DiscordButtonComponent noButton =
            new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, noId, "No", false, null);

        DiscordWebhookBuilder responseBuilder = new();
        responseBuilder.AddEmbed(embed.Build());
        responseBuilder.AddComponents(yesButton, noButton);

        var response = await context.EditResponseAsync(responseBuilder);
        var interactivity = bot.Client.GetExtension<InteractivityExtension>();

        var interaction = await interactivity.WaitForButtonAsync(response, new[] { yesButton, noButton },
            TimeSpan.FromMinutes(3));

        await context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Acknowledged"));

        if(interaction.TimedOut)
        {
            await context.SendTimeout("You took too long to answer my question");
            return false;
        }

        return interaction.Result.Id.ToLower().EndsWith("yes");
    }

    /// <summary>
    /// Retrieve a value of specific type from user
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="message">Prompt for user to answer</param>
    /// <param name="bot"></param>
    /// <returns>Resposne from user</returns>
    public static async Task<T> GetValue<T>(this InteractionContext context, string message, IDiscordHostedService bot)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Require Input")
            .WithColor(DiscordColor.Cyan)
            .WithFooter("Please respond like a normal message")
            .WithDescription(message);

        DiscordEmbedBuilder errorEmbed = new DiscordEmbedBuilder()
            .WithTitle("Invalid Input")
            .WithColor(DiscordColor.Red)
            .WithFooter("Please respond with the right stuff this time...");

        bool valid = true;
        string errorMessage = "";

        var interactivity = bot.Client.GetExtension<InteractivityExtension>();

        do
        {
            try
            {
                DiscordWebhookBuilder responseBuilder = new();
                responseBuilder.AddEmbed(string.IsNullOrEmpty(errorMessage) ? embed : errorEmbed);
                await context.EditResponseAsync(responseBuilder);

                var nextMessage = await interactivity.WaitForMessageAsync(x =>
                    x.Author.Id == context.User.Id && x.ChannelId == context.Channel.Id, TimeSpan.FromMinutes(5));

                if (nextMessage.TimedOut)
                {
                    await nextMessage.Result.DeleteAsync();
                    return default;
                }

                T value = (T)Convert.ChangeType(nextMessage.Result.Content, typeof(T));
                await nextMessage.Result.DeleteAsync();

                return value;
            }
            catch (Exception ex)
            {
                valid = false;
                errorMessage = $"Invalid Input. Require something that can convert into a {typeof(T).Name}";
            }
        } while (!valid);

        return default;
    }

    /// <summary>
    /// Update the interaction component to a 'thinking' state
    /// This provides visual feedback that the user is now waiting server response
    /// </summary>
    /// <param name="context"></param>
    public static async Task ImThinking(this InteractionContext context)
    {
        // Lets the user know that we got the command and that the bot is 'thinking' -- or really just taking a long time
        await context.CreateResponseAsync(DisCatSharp.InteractionResponseType.DeferredChannelMessageWithSource, new()
        {
            IsEphemeral = true
        });
    }
}
