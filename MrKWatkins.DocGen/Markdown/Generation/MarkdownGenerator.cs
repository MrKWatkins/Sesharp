using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Markdown.Generation;

public abstract class MarkdownGenerator
{
    protected MarkdownGenerator(MemberLookup memberLookup, string parentDirectory)
    {
        MemberLookup = memberLookup;
        ParentDirectory = parentDirectory;
    }

    protected MemberLookup MemberLookup { get; }

    protected string ParentDirectory { get; }

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
        var text = see.Text;
        if (text != null)
        {
            writer.WriteLink(text, see.Id.ToString());
            return;
        }

        var (member, location) = MemberLookup.Get(see.Id);
        switch (location)
        {
            case MemberLocation.DocumentAssembly:
                writer.WriteLink(member.DisplayName(), member.DocumentationFileName());
                break;
            case MemberLocation.System:
                writer.WriteLink(member.DisplayName(), member.MicrosoftFileName());
                break;
            default:
                writer.Write(member.DisplayName());
                break;
        }
    }
}