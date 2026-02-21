using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class OperatorMarkdownGenerator(IFileSystem fileSystem, MemberLookup memberLookup, string outputDirectory) : MemberMarkdownGenerator<Operator, MethodInfo>(fileSystem, memberLookup, outputDirectory)
{
    protected override void Generate(MarkdownWriter writer, Operator @operator)
    {
        writer.WriteMainHeading($"{@operator.Type.DisplayName}.{@operator.DisplayName} Operator");

        writer.WriteSubHeading("Definition");

        WriteOperator(writer, @operator);
    }

    protected override void Generate(MarkdownWriter writer, MemberGroup<Operator, MethodInfo> group)
    {
        writer.WriteMainHeading($"{group.Type.DisplayName}.{group.DisplayName} Operator");

        WriteMemberTable(writer, group.Members, "Overloads");

        foreach (var @operator in group.Members)
        {
            var id = MarkdownId.FromMember(@operator.MemberInfo);

            writer.WriteSubHeading(@operator.MemberName, id);

            // False positive on Dispose pattern.
#pragma warning disable CA2000
            using var _ = writer.WithIdSuffix(id);
#pragma warning restore CA2000

            WriteOperator(writer, @operator);
        }
    }

    private void WriteOperator(MarkdownWriter writer, Operator @operator)
    {
        WriteSection(writer, @operator.Documentation?.Summary);

        WriteSignature(writer, @operator);

        WriteParameters(writer, @operator, @operator.Parameters);

        WriteReturns(writer, @operator, @operator.MemberInfo.ReturnType);

        WriteRemarks(writer, @operator.Documentation);
    }

    private static void WriteSignature(MarkdownWriter writer, Operator @operator)
    {
        using var code = writer.CodeBlock();

        code.Write("public static ");

        WriteTypeOrKeyword(code, @operator.MemberInfo.ReturnType);
        if (@operator.MemberInfo.IsReturnNullableReferenceType())
        {
            code.Write("?");
        }
        code.Write(" ");

        code.Write("operator ");
        code.Write(@operator.DisplayName);

        code.Write("(");
        WriteSignatureParameters(code, @operator.Parameters);
        code.Write(");");
    }
}
