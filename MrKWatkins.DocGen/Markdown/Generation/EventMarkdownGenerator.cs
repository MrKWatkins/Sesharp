using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class EventMarkdownGenerator : MemberMarkdownGenerator<EventInfo, Event>
{
    public EventMarkdownGenerator(MemberLookup memberLookup, string parentDirectory)
        : base(memberLookup, parentDirectory)
    {
    }
}