namespace MrKWatkins.Sesharp.XmlDocumentation;

public sealed class CodeElement(string code) : DocumentationElement
{
    public string Code { get; } = code;
}