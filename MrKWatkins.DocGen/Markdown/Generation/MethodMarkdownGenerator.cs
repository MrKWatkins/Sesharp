using System.Reflection;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class MethodMarkdownGenerator : MemberMarkdownGenerator<MethodBase, Method>
{
    public MethodMarkdownGenerator(TypeLookup typeLookup, string parentDirectory)
        : base(typeLookup, parentDirectory)
    {
    }
}