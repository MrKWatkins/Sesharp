using System.Reflection;
using Humanizer;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class MemberMarkdownGenerator<TMember, TMemberInfo> : MarkdownGenerator
    where TMember : Member<TMemberInfo>
    where TMemberInfo : MemberInfo
{
    protected MemberMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    public void Generate([InstantHandle] IEnumerable<OutputNode> nodes)
    {
        foreach (var node in nodes)
        {
            Generate(node);
        }
    }

    public sealed override void Generate(OutputNode node)
    {
        if (node is not TMember and not MemberGroup<TMember, TMemberInfo>)
        {
            throw new ArgumentException($"Value must be a {typeof(TMember).Name} or a group of {typeof(TMember).Name.Pluralize()}.", nameof(node));
        }

        using var writer = CreateWriter(node);

        if (node is TMember member)
        {
            Generate(member, writer);
        }
        else
        {
            Generate((MemberGroup<TMember, TMemberInfo>)node, writer);
        }
    }

    protected virtual void Generate(TMember member, MarkdownWriter writer) =>
        throw new NotSupportedException($"Single {typeof(TMember).Name.Pluralize()} are not supported.");

    protected virtual void Generate(MemberGroup<TMember, TMemberInfo> group, MarkdownWriter writer) =>
        throw new NotSupportedException($"Groups of {typeof(TMember).Name.Pluralize()} are not supported.");

    protected string Name => typeof(TMember).Name;

    protected string PluralName => typeof(TMember).Name.Pluralize();

    public void WriteTypeSection(MarkdownWriter writer, IReadOnlyList<TMember> members)
    {
        if (members.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading(PluralName);

        using var table = writer.Table("Name", "Description");

        foreach (var member in members)
        {
            table.NewRow();
            table.Write(member.MemberName);
            table.NewColumn();

            WriteSection(table, member.Documentation?.Summary);
        }
    }
}