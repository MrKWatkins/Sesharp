namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class CodeElement : DocumentationElement
{
    public CodeElement(string code)
    {
        Code = code;
    }

    public string Code { get; }
}