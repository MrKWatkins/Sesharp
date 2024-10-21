using System.Reflection;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class ConstructorMarkdownGenerator(MemberLookup memberLookup, string outputDirectory) : MemberMarkdownGenerator<Constructor, ConstructorInfo>(memberLookup, outputDirectory)
{
    protected override void Generate(MarkdownWriter writer, MemberGroup<Constructor, ConstructorInfo> group)
    {
        writer.WriteMainHeading($"{group.Type.DisplayName} Constructors");

        if (group.Members.Count == 1)
        {
            WriteConstructor(writer, group.Members[0]);
            return;
        }

        WriteMemberTable(writer, group.Members, "Overloads", c => c.TitleName);

        foreach (var constructor in group.Members)
        {
            var id = MarkdownId.FromMember(constructor.MemberInfo);

            writer.WriteSubHeading(constructor.TitleName, id);

            // False positive on Dispose pattern.
#pragma warning disable CA2000
            using var _ = writer.WithIdSuffix(id);
#pragma warning restore CA2000

            WriteConstructor(writer, constructor);
        }
    }

    private void WriteConstructor(MarkdownWriter writer, Constructor constructor)
    {
        WriteSection(writer, constructor.Documentation?.Summary);

        WriteSignature(writer, constructor);

        WriteTypeParameters(writer, constructor, constructor.TypeParameters);

        WriteParameters(writer, constructor, constructor.Parameters);

        WriteRemarks(writer, constructor.Documentation);
    }

    private static void WriteSignature(MarkdownWriter writer, Constructor constructor)
    {
        using var code = writer.CodeBlock();

        if (constructor.IsStatic)
        {
            code.Write("static ");
        }
        else
        {
            code.Write(constructor.Accessibility.ToCSharpKeywords());
            code.Write(" ");
        }

        code.Write(constructor.Type.NonGenericName);

        code.Write("(");
        WriteSignatureParameters(code, constructor.Parameters);
        code.Write(");");
    }
}