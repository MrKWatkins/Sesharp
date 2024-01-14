using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Markdown.Generation;

public abstract class MarkdownGenerator
{
    protected MarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
    {
        MemberLookup = memberLookup;
        OutputDirectory = outputDirectory;
    }

    public abstract void Generate(OutputNode node);

    protected MemberLookup MemberLookup { get; }

    protected string OutputDirectory { get; }

    [MustUseReturnValue]
    protected MarkdownWriter CreateWriter(OutputNode node)
    {
        var filePath = Path.Combine(OutputDirectory, node.FileName);
        return new MarkdownWriter(filePath);
    }

    protected void WriteRemarks(MarkdownWriter writer, MemberDocumentation? documentation)
    {
        if (documentation?.Remarks == null)
        {
            return;
        }

        writer.WriteSubHeading("Remarks");

        using var paragraph = writer.Paragraph();
        WriteSection(paragraph, documentation.Remarks);
    }

    protected void WriteSection(MarkdownWriter writer, DocumentationSection? section)
    {
        if (section == null)
        {
            return;
        }

        using var paragraph = writer.Paragraph();
        WriteSection(paragraph, section);
    }

    protected void WriteSection(IParagraphWriter writer, DocumentationSection? section)
    {
        if (section == null)
        {
            return;
        }

        foreach (var element in section.Elements)
        {
            switch (element)
            {
                case CodeElement codeElement:
                    writer.WriteCode(codeElement.Code);
                    break;
                case ParamRef paramRef:
                    writer.WriteCode(paramRef.Name);
                    break;
                case See see:
                    WriteSee(writer, see);
                    break;
                case TypeParamRef typeParamRef:
                    writer.WriteCode(typeParamRef.Name);
                    break;
                case TextElement textElement:
                    writer.Write(textElement.Text);
                    break;
                default:
                    throw new NotSupportedException($"The {nameof(DocumentationElement)} {element.GetType()} is not supported.");
            }
        }
    }

    private void WriteSee(IParagraphWriter writer, See see)
    {
        var (member, location) = MemberLookup.Get(see.Id);
        WriteMemberLink(writer, member, location, see.Text);
    }

    protected void WriteMemberLink(IParagraphWriter writer, MemberInfo member, string? text = null) =>
        WriteMemberLink(writer, member, MemberLookup.GetLocation(member), text);

    protected static void WriteMemberLink(IParagraphWriter writer, MemberInfo member, MemberLocation location, string? text = null)
    {
        text ??= member.DisplayName();

        switch (location)
        {
            case MemberLocation.DocumentAssembly:
                writer.WriteLink(text, member.DocumentationFileName());
                break;
            case MemberLocation.System:
                writer.WriteLink(text, member.MicrosoftUrl());
                break;
            default:
                writer.Write(text);
                break;
        }
    }
}