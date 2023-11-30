using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class FieldMarkdownGenerator : MemberMarkdownGenerator<FieldInfo, Field>
{
    public FieldMarkdownGenerator(TypeLookup typeLookup, string parentDirectory)
        : base(typeLookup, parentDirectory)
    {
    }
}