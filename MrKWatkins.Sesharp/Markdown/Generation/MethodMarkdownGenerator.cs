using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class MethodMarkdownGenerator : MemberMarkdownGenerator<Method, MethodInfo>
{
    public MethodMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    protected override void Generate(MarkdownWriter writer, Method method)
    {
        writer.WriteMainHeading($"{method.Type.DisplayName}.{method.DisplayName} Method");

        writer.WriteSubHeading("Definition");

        WriteMethod(writer, method);
    }

    private void WriteMethod(MarkdownWriter writer, Method method)
    {
        WriteSection(writer, method.Documentation?.Summary);

        WriteSignature(writer, method);

        WriteTypeParameters(writer, method, method.TypeParameters);

        WriteParameters(writer, method, method.Parameters);

        WriteReturns(writer, method, method.MemberInfo.ReturnType);

        WriteRemarks(writer, method.Documentation);
    }

    protected override void Generate(MarkdownWriter writer, MemberGroup<Method, MethodInfo> group)
    {
        writer.WriteMainHeading($"{group.Type.DisplayName}.{group.DisplayName} Method");

        WriteMemberTable(writer, group.Members, "Overloads");

        foreach (var method in group.Members)
        {
            var id = MarkdownId.FromMember(method.MemberInfo);

            writer.WriteSubHeading(method.MemberName, id);

            // False positive on Dispose pattern.
#pragma warning disable CA2000
            using var _ = writer.WithIdSuffix(id);
#pragma warning restore CA2000

            WriteMethod(writer, method);
        }
    }

    private static void WriteSignature(MarkdownWriter writer, Method method)
    {
        using var code = writer.CodeBlock();

        WriteAccessibility(code, method.Accessibility);
        code.Write(" ");

        if (method.IsStatic)
        {
            code.Write("static ");
        }

        if (method.Virtuality != Virtuality.Normal)
        {
            code.Write(method.Virtuality.ToCSharpKeywords());
            code.Write(" ");
        }


        WriteTypeOrKeyword(code, method.MemberInfo.ReturnType);
        if (method.MemberInfo.IsReturnNullableReferenceType())
        {
            code.Write("?");
        }
        code.Write(" ");

        code.Write(method.Name);

        WriteSignatureTypeParameters(code, method.MemberInfo.GetGenericArguments());

        code.Write("(");
        WriteSignatureParameters(code, method.Parameters);
        code.Write(")");

        WriteSignatureTypeConstraints(code, method.MemberInfo.GetGenericArguments());

        code.Write(";");
    }
}