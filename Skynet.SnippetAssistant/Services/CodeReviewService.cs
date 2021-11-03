using Skynet.SnippetAssistant.Interfaces;
using System.Net;

namespace Skynet.SnippetAssistant.Services;
using Reports;
using Skynet.SnippetAssistant.Reports.Python;
using System.Diagnostics;

internal class CodeReviewService : ICodeReviewService
{
    private readonly ISnippetStorageService _snippetStorage;

    public CodeReviewService(ISnippetStorageService storage)
    {
        _snippetStorage = storage;
    }

    public Dictionary<string, string> SupportedLanguages => new()
    {
        { "py", "Python" }
    };

    public string GetLanguage(string fileExtension)
    {
        if (SupportedLanguages.ContainsKey(fileExtension))
            return SupportedLanguages[fileExtension];

        return fileExtension;
    }

    public string GetExtension(string language)
    {
        if (SupportedLanguages.ContainsValue(language))
            return SupportedLanguages.First(x => x.Value == language).Key;

        return language;
    }

    public async Task DownloadFileAsync(string url, string downloadDestination)
    {
        using WebClient client = new();
        await client.DownloadFileTaskAsync(url, downloadDestination);
    }

    public async Task<ReportBase> AnalyzeAsync(string codeFilePath, string? language = null)
    {
        if (string.IsNullOrEmpty(language))
            language = GetLanguage(new FileInfo(codeFilePath).Extension.Replace(".",""));

        string results = await ExecuteProspectorTool(language, codeFilePath);
        string originalCode = await File.ReadAllTextAsync(codeFilePath);

        ReportBase report = null;

        switch(language.ToLower())
        {
            case "python":
                var deserializedOutput = Newtonsoft.Json.JsonConvert.DeserializeObject<PythonOutput>(results);
                report = new PythonReport(deserializedOutput, originalCode);
                break;
            default:
                throw new NotImplementedException($"{language} has not been implemented yet");
        }

        return report;
    }

    public Task CleanupAsync(int deleteAfterMinutes, string originalFile, string reportFile)
    {
        if (File.Exists(originalFile))
            File.Delete(originalFile);

        return Task.CompletedTask;
    }

    async Task<string> ExecuteProspectorTool(string language, string codeFilePath)
    {
        Process toolProcess = new();
        string profilePath = Path.Join(_snippetStorage.ToolsProfilePath, language, "ProspectorProfile.yml");

        ProcessStartInfo startInfo = new()
        {
            FileName = "prospector",
            Arguments = $"--profile-path \"{profilePath}\" --output-format json \"{codeFilePath}\"",
            RedirectStandardOutput = true
        };

        toolProcess.StartInfo = startInfo;
        toolProcess.Start();
        await toolProcess.WaitForExitAsync();
        return await toolProcess.StandardOutput.ReadToEndAsync();
    }
}
