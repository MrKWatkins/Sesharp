using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class PropertyMarkdownGenerator : MemberMarkdownGenerator<PropertyInfo, Property>
{
    public PropertyMarkdownGenerator(MemberLookup memberLookup, string parentDirectory)
        : base(memberLookup, parentDirectory)
    {
    }
}