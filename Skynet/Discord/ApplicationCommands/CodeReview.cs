namespace Skynet.Discord.ApplicationCommands;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.Hosting;
using DisCatSharp.Entities;
using SnippetAssistant.Interfaces;
using Options;
using Skynet.Discord.Extensions;
using DisCatSharp.Interactivity;
using Skynet.Core.Interfaces;

public class CodeReview : ApplicationCommandsModule
{
    public ILogger<CodeReview> Logger { get; set; }
    public IDiscordHostedService Bot { get; set; }
    public DiscordOptions DiscordOptions { get; set; }
    public IConfiguration Configuration { get; set; }
    public IStorageService StorageHandler { get; set; }
    public ICodeReviewService CodeReviewService { get; set; }


    [SlashCommand("code-review", "Automated review of code")]
    public async Task Command(InteractionContext context)
    {
        if (!await context.ValidateGuild())
            return;

        var interactivity = Bot.Client.GetExtension<InteractivityExtension>();
        
        await context.ImThinking();

        #region Start Process of getting user input
        DiscordWebhookBuilder messageBuilder = new();
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Code Review")
            .WithDescription("Please respond to this message with an attached code file (or zip of python files)")
            .WithColor(DiscordColor.Aquamarine);

        messageBuilder.AddEmbed(embedBuilder.Build());
        var inputInquiry = await context.EditResponseAsync(messageBuilder);
        #endregion

        #region Process user inquiry response
        var inquiryResponse = await interactivity.WaitForMessageAsync(x =>
        {
            if (x.Author != context.User)
                return false;

            if (!x.Attachments.Any())
                return false;

            return true;
        }, TimeSpan.FromMinutes(5));

        if(inquiryResponse.TimedOut)
        {
            messageBuilder = new();
            embedBuilder.Description = "I'm sorry, either time ran out or you did not attach a file";
            embedBuilder.Color = DiscordColor.Red;
            messageBuilder.AddEmbed(embedBuilder.Build());

            await context.EditResponseAsync(messageBuilder);
            return;
        }

        DiscordAttachment attachment = inquiryResponse.Result.Attachments.First();
        string attachmentPath = StorageHandler.GetPathFromRoot("TemporaryFileStorage", string.Join('_', context.User.Username, attachment.FileName));
        string language = CodeReviewService.GetLanguage(attachment.FileName.Split('.').Last());

        if(!CodeReviewService.SupportedLanguages.ContainsKey(language))
        {
            messageBuilder = new();
            embedBuilder.Description = "I'm sorry but we do not support that file extension. Currently we support the following:\n\t" +
               string.Join("\n\t", CodeReviewService.SupportedLanguages.Select(x => x.Key));
            embedBuilder.Color = DiscordColor.Red;
            messageBuilder.AddEmbed(embedBuilder.Build());
            await context.EditResponseAsync(messageBuilder);
            return;
        }

        try
        {
            // Download the user's file
            await CodeReviewService.DownloadFileAsync(attachment.Url, attachmentPath);

            // analyze the provided file
            var report = await CodeReviewService.AnalyzeAsync(attachmentPath, language);

            string reportFileName = $"{context.User.Username.Replace(" ", "")}_{attachment.FileName.Split('.').First()}_report.html";
            string reportFilePath = StorageHandler.GetPathFromRoot("GeneratedReports", reportFileName);
            await File.WriteAllTextAsync(reportFilePath, await report.GenerateReportAsync());

            embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Code Review")
                .WithDescription($"{context.User.Username}, based on {language} standards this is what I suggest.\n" +
                $"Please view the HTML in browser!")
                .WithFooter(context.User.Username)
                .WithColor(DiscordColor.Purple);

            using Stream stream = new FileStream(reportFilePath, FileMode.Open);
            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithEmbed(embedBuilder.Build())
                .WithFile(reportFileName, stream);

            stream.Close();
            await stream.DisposeAsync();
            await context.Channel.SendMessageAsync(builder);

            Logger.LogInformation($"Cleaning up user provided file from {context.User.Username} - {attachment.FileName}");
            await inquiryResponse.Result.DeleteAsync();
            await CodeReviewService.CleanupAsync(1, attachmentPath, reportFilePath);
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, $"Something happened while processing {attachment.FileName}");
        }
        #endregion
    }
}
