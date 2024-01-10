using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class EventMarkdownGenerator : MemberMarkdownGenerator<Event, EventInfo>
{
    public EventMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(Event member, MarkdownWriter writer)
    {
    }
}