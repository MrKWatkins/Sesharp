using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class ConstructorMarkdownGenerator : MemberMarkdownGenerator<MethodBase, Constructor>
{
    public ConstructorMarkdownGenerator(TypeLookup typeLookup, string parentDirectory)
        : base(typeLookup, parentDirectory)
    {
    }
}