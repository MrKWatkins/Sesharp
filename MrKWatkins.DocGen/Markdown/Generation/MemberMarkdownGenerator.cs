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
            Generate(writer, member);
        }
        else
        {
            Generate(writer, (MemberGroup<TMember, TMemberInfo>)node);
        }
    }

    protected virtual void Generate(MarkdownWriter writer, TMember member) =>
        throw new NotSupportedException($"Single {typeof(TMember).Name.Pluralize()} are not supported.");

    protected virtual void Generate(MarkdownWriter writer, MemberGroup<TMember, TMemberInfo> group) =>
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
            WriteMemberLink(table, member.MemberInfo);
            table.NewColumn();

            WriteSection(table, member.Documentation?.Summary);
        }
    }

    protected void WriteParameters(MarkdownWriter writer, DocumentableNode member, IReadOnlyList<Parameter> parameters)
    {
        if (parameters.Count == 0)
        {
            return;
        }

        writer.WriteSubHeading("Parameters");

        using var table = writer.Table("Name", "Type", "Description");

        var documentation = member.Documentation;
        foreach (var parameter in parameters)
        {
            table.NewRow();
            table.Write(parameter.Name);
            table.NewColumn();
            WriteMemberLink(table, parameter.Type);
            table.NewColumn();

            var parameterDocumentation = documentation?.Parameters.GetValueOrDefault(parameter.Name);
            WriteSection(table, parameterDocumentation);
        }
    }

    protected void WriteReturns(MarkdownWriter writer, DocumentableNode member, System.Type returnType, string sectionName = "Returns")
    {
        writer.WriteSubHeading(sectionName);

        using (var typeParagraph = writer.Paragraph())
        {
            WriteMemberLink(typeParagraph, returnType);
        }

        var returns = member.Documentation?.Returns;
        if (returns == null)
        {
            return;
        }

        using (var returnsParagraph = writer.Paragraph())
        {
            WriteSection(returnsParagraph, returns);
        }
    }
}