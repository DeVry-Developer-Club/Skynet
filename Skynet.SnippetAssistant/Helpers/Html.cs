namespace Skynet.SnippetAssistant.Helpers;
internal static class Html
{
    /// <summary>
    /// Generates Discord code block for the necessary language
    /// </summary>
    /// <param name="language">Language utilized by highlightjs</param>
    /// <param name="code">Code that shall appear within the code block</param>
    /// <param name="padding">Optional padding to provide (default is p-3)</param>
    /// <returns>HighlightJS formatted code block</returns>
    public static string CreateCodeBlock(string language, string code, string padding = "p-3") =>
        $"<div class=\"container {padding}\"><pre><code class=\"language-{language}\">{code}</code></pre></div>";
}
