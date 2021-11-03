namespace Skynet.SnippetAssistant.Reports.Python;
internal class PythonOutput : Core.IOutput
{
    public PythonSummary Summary { get; set; }
    public PythonMessage[] Messages { get; set; }
}
