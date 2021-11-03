namespace Skynet.SnippetAssistant.Interfaces;
internal interface ISnippetStorageService
{
    /// <summary>
    /// Path to profiles used for various static analysis tools
    /// </summary>
    string ToolsProfilePath { get; }

    /// <summary>
    /// Path to generated reports
    /// </summary>
    string GeneratedReportsPath { get; }
}
