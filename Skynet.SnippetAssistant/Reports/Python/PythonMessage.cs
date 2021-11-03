﻿using Skynet.SnippetAssistant.Reports.Core;

namespace Skynet.SnippetAssistant.Reports.Python;

internal class PythonMessage : IMessage
{
    public string Source { get; set; }
    public string Code { get; set; }
    public PythonLocation Location { get; set; }
    public string Message { get; set; }
    public string CommentedCode { get; set; }
}

internal class PythonLocation
{
    public string Path { get; set; }
    public string Module { get; set; }
    public string Function { get; set; }
    public int? Line { get; set; }
    public int? Character { get; set; }
}