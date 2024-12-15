using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class EventMarkdownGenerator(IFileSystem fileSystem, MemberLookup memberLookup, string outputDirectory) : MemberMarkdownGenerator<Event, EventInfo>(fileSystem, memberLookup, outputDirectory)
{
    protected override void Generate(MarkdownWriter writer, Event @event)
    {
        writer.WriteMainHeading($"{@event.Type.DisplayName}.{@event.DisplayName} Event");
    }
}