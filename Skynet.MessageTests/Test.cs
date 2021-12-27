using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EngineerNotebook.Shared.Endpoints.Tag;
using Xunit;
using EngineerNotebook.Shared.Models;

namespace Skynet.MessageTests;

public class Test
{
    [Fact]
    void Test2()
    {
        TagDto[] tags =
        {
          new()
          {
              Name = "install",
              TagType = TagType.Prefix
          },
          new()
          {
              Name = "ai",
              TagType = TagType.Value
          },
          new()
          {
              Name = "uninstall",
              TagType = TagType.Prefix
          },
          new()
          {
              Name = "download",
              TagType = TagType.Prefix
          },
          new()
          {
              Name = "python",
              TagType = TagType.Value
          },
          new()
          {
              Name = "c#",
              TagType = TagType.Value
          },
          new()
          {
              Name = "test me",
              TagType = TagType.Phrase
          }
        };

        const string format = @"\b{0}\b";
        string prefixes = string.Join("|", tags.Where(x => x.TagType == TagType.Prefix).Select(x => string.Format(format, x.Name)));
        string values = string.Join("|", tags.Where(x => x.TagType == TagType.Value).Select(x => string.Format(format, x.Name)));
        
        Regex regex = new($"({prefixes}).*?({values})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        string[] messages =
        {
            "python install",
            "install python",
            "install some python",
            "do not install",
            "install something",
            "something test",
            "I want ai to install C# for python"
        };
        
        Assert.DoesNotMatch(regex, messages[0]);
        Assert.Matches(regex, messages[1]);
        Assert.Matches(regex, messages[2]);
        Assert.DoesNotMatch(regex, messages[3]);
        Assert.DoesNotMatch(regex, messages[4]);
        Assert.DoesNotMatch(regex, messages[5]);

        var matches = regex.Matches(messages[^1]);
        Assert.Equal(2, matches.Count);
    }
}