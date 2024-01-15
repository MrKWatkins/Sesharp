using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;

namespace MrKWatkins.DocGen.Markdown.Generation;

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
            writer.WriteSubHeading(method.MemberName);

            WriteMethod(writer, method);
        }
    }

    private static void WriteSignature(MarkdownWriter writer, Method method)
    {
        using var code = writer.CodeBlock();

        code.Write(method.Visibility.ToKeyword());
        code.Write(" ");

        if (method.IsStatic)
        {
            code.Write("static ");
        }

        if (method.Virtuality.HasValue)
        {
            code.Write(method.Virtuality.Value.ToKeyword());
            code.Write(" ");
        }

        code.Write(method.MemberInfo.ReturnType.DisplayNameOrKeyword());
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