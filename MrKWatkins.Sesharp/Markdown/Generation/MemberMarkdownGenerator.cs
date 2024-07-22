using System.Reflection;
using Humanizer;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;

namespace MrKWatkins.Sesharp.Markdown.Generation;

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
        switch (node)
        {
            case TMember member:
                if (!ShouldGenerate(member))
                {
                    break;
                }

                using (var writer = CreateWriter(node))
                {
                    Generate(writer, member);
                    break;
                }

            case MemberGroup<TMember, TMemberInfo> memberGroup:
                using (var writer = CreateWriter(node))
                {
                    Generate(writer, memberGroup);
                    break;
                }

            default:
                throw new ArgumentException($"Value must be a {typeof(TMember).Name} or a group of {typeof(TMember).Name.Pluralize()}.", nameof(node));
        }
    }

    protected virtual bool ShouldGenerate(TMember member) => true;

    protected virtual void Generate(MarkdownWriter writer, TMember member) =>
        throw new NotSupportedException($"Single {typeof(TMember).Name.Pluralize()} are not supported.");

    protected virtual void Generate(MarkdownWriter writer, MemberGroup<TMember, TMemberInfo> group) =>
        throw new NotSupportedException($"Groups of {typeof(TMember).Name.Pluralize()} are not supported.");

    protected string Name => typeof(TMember).Name;

    protected string PluralName => typeof(TMember).Name.Pluralize();

    public void WriteMemberTable(MarkdownWriter writer, IReadOnlyList<TMember> members, string? heading = null, Func<TMember, string>? getMemberName = null)
    {
        if (members.Count == 0)
        {
            return;
        }

        getMemberName ??= m => m.MemberName;

        writer.WriteSubHeading(heading ?? PluralName);

        using var table = writer.Table("Name", "Description");

        foreach (var member in members)
        {
            table.NewRow();
            WriteMemberLink(table, member.MemberInfo, getMemberName(member));
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
        if (returnType == typeof(void))
        {
            return;
        }

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