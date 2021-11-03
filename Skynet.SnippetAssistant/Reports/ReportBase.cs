using Razor.Templating.Core;
using Skynet.SnippetAssistant.Helpers;
namespace Skynet.SnippetAssistant.Reports;

public abstract class ReportBase
{
    /// <summary>
    /// Title of report
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Compiled HTML Contents
    /// </summary>
    public string Contents { get; private set; }

    /// <summary>
    /// Type of language that shall be used for highlighting purposes
    /// </summary>
    public readonly string Language;

    /// <summary>
    /// Original code the user provided
    /// </summary>
    protected readonly string OriginalCode;

    protected ReportBase(string fileContents, string language)
    {
        Language = language;
        OriginalCode = fileContents;
    }

    /// <summary>
    /// Generates the HTML page that the user can view
    /// </summary>
    /// <returns>Compiled HTML from the Main.cshtml template</returns>
    public async Task<string> GenerateReportAsync()
    {
        Contents = await GenerateSummaryAsync() +
            await GenerateErrorReportAsync() +
            Html.CreateCodeBlock(Language, OriginalCode);

        return await RazorTemplateEngine.RenderAsync("~/Views/Shared/Main.cshtml", this);
    }

    /// <summary>
    /// Generate summary block that appears on the top of the report
    /// </summary>
    /// <returns>Compiled HTML</returns>
    protected virtual Task<string> GenerateSummaryAsync() => Task.FromResult("<p>Default Summary</p>");

    /// <summary>
    /// Generate the error/suggestion messages
    /// </summary>
    /// <returns></returns>
    protected virtual Task<string> GenerateErrorReportAsync() => Task.FromResult("<p>No errors to report</p>");
}
