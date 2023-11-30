using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Markdown.Generation;

public abstract class MarkdownGenerator
{
    protected MarkdownGenerator(TypeLookup typeLookup, string parentDirectory)
    {
        TypeLookup = typeLookup;
        ParentDirectory = parentDirectory;
    }

    protected TypeLookup TypeLookup { get; }

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
                    writer.WriteCode(paramRef.Key);
                    break;
                case See see:
                    WriteSee(writer, see);
                    break;
                case TypeParamRef typeParamRef:
                    writer.WriteCode(typeParamRef.Key);
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
            writer.WriteLink(text, see.Key);
            return;
        }

        var reference = MemberReference.Parse(TypeLookup, see.Key);
        switch (reference.Location)
        {
            case TypeLocation.DocumentAssembly:
                writer.WriteLink(reference.Type.DisplayName(), reference.Type.DocumentationFileName());
                break;
            case TypeLocation.System:
                writer.WriteLink(reference.Type.DisplayName(), reference.Type.MicrosoftFileName());
                break;
            default:
                writer.Write(reference.Type.DisplayName());
                break;
        }
    }
}