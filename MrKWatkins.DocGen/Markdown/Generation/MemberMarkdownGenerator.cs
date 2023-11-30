using System.Reflection;
using Humanizer;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class MemberMarkdownGenerator<TMemberInfo, TMember> : MarkdownGenerator
    where TMemberInfo : MemberInfo
    where TMember : DocumentableNode<TMemberInfo>
{
    protected MemberMarkdownGenerator(TypeLookup typeLookup, string parentDirectory)
        : base(typeLookup, parentDirectory)
    {
    }

    protected string Name => typeof(TMember).Name;

    protected string PluralName => typeof(TMember).Name.Pluralize();

    public void WriteTypeSection(MarkdownWriter writer, IReadOnlyList<TMember> members)
    {
        if (members.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading(PluralName);

        using var table = writer.Table();

        foreach (var member in members)
        {
            table.NewRow();
            table.Write(member.DisplayName);
            table.NewColumn();

            WriteSection(table, member.Documentation?.Summary);
        }
    }
}