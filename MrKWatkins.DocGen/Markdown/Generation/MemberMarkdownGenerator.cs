using System.Reflection;
using Humanizer;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class MemberMarkdownGenerator<TMember, TMemberInfo> : MarkdownGenerator<TMember>
    where TMember : DocumentableNode<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    protected MemberMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    public sealed override void Generate(TMember member)
    {
        var filePath = Path.Combine(OutputDirectory, member.FileName);
        using var writer = new MarkdownWriter(filePath);
        Generate(member, writer);
    }

    protected abstract void Generate(TMember member, MarkdownWriter writer);

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