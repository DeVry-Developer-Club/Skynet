namespace Skynet.SnippetAssistant.Interfaces;
public interface ICodeReviewService
{
    /// <summary>
    /// Languages that are currently supported by the review service
    /// </summary>
    Dictionary<string, string> SupportedLanguages { get; }

    /// <summary>
    /// Determine language by file extension
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns>Language associated with file extension</returns>
    string GetLanguage(string fileExtension);

    /// <summary>
    /// Get the file extension based on <paramref name="language"/>
    /// </summary>
    /// <param name="language"></param>
    /// <returns>File extension associated with <paramref name="language"/></returns>
    string GetExtension(string language);

    /// <summary>
    /// Download submitted file for processing
    /// </summary>
    /// <param name="url">Retrieves file from url</param>
    /// <param name="downloadDestination">Puts downloaded file into designated location</param>
    Task DownloadFileAsync(string url, string downloadDestination);

    /// <summary>
    /// Analyze a given file for a particular language
    /// </summary>
    /// <param name="codeFilePath">Path to file to analyze</param>
    /// <param name="language">Language of file</param>
    /// <returns>Report for specified file and language</returns>
    Task<Reports.ReportBase> AnalyzeAsync(string codeFilePath, string? language = null);

    /// <summary>
    /// Cleanup the files after a specified amount of time
    /// </summary>
    /// <param name="deleteAfterMinutes">Delete after given time</param>
    /// <param name="originalFile">Path to original file</param>
    /// <param name="reportFile">Path to report based on original file </param>
    Task CleanupAsync(int deleteAfterMinutes, string originalFile, string reportFile);
}
