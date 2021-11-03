namespace Skynet.Discord.Attributes;
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InteractionNameAttribute : Attribute
{
    public string Name { get; }

    public InteractionNameAttribute(string name)
    {
        Name = name;
    }
}
