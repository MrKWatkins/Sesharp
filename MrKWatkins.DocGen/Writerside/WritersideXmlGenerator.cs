using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using MrKWatkins.DocGen.Model;
using Type = MrKWatkins.DocGen.Model.Type;

namespace MrKWatkins.DocGen.Writerside;

public static class WritersideXmlGenerator
{
    public static void UpdateWriterside(WritersideOptions options, AssemblyDetails assemblyDetails)
    {
        var toc = GenerateToc(options, assemblyDetails);

        var hiTree = XDocument.Load(options.TreeFilePath);

        var existingToc = hiTree.XPathSelectElement($"//*[@id='{options.TocElementId}']")
                          ?? throw new InvalidOperationException($"Could not find element with ID {options.TocElementId}.");

        existingToc.ReplaceWith(toc);

        var xmlSettings = new XmlWriterSettings
        {
            Indent = true
        };

        using var writer = XmlWriter.Create(options.TreeFilePath, xmlSettings);
        hiTree.Save(writer);
    }

    [Pure]
    public static XElement GenerateToc(WritersideOptions options, AssemblyDetails assemblyDetails)
    {
        var toc = CreateTitleElement(options.TocElementTitle);
        toc.SetAttributeValue("id", options.TocElementId);

        foreach (var @namespace in assemblyDetails.Namespaces)
        {
            toc.Add(CreateNamespaceElement(@namespace));
        }

        return toc;
    }

    [Pure]
    private static XElement CreateNamespaceElement(Namespace @namespace)
    {
        var element = CreateTitleElement(@namespace.Name);
        foreach (var type in @namespace.Types)
        {
            element.Add(CreateTypeElement(type));
        }
        return element;
    }

    [Pure]
    private static XElement CreateTypeElement(Type type)
    {
        if (type.MemberInfo.IsEnum)
        {
            return CreateMemberTocElement(type);
        }

        var title = CreateTitleElement(type.DisplayName);
        title.Add(CreateMemberTocElement(type));
        if (type.ConstructorGroup != null)
        {
            title.Add(CreateTocElement("Constructors", type.ConstructorGroup.FileName));
        }
        title.Add(CreateMemberElements("Fields", type.Fields));
        title.Add(CreateMemberElements("Properties", type.Properties));
        title.Add(CreateMemberElements("Methods", type.Methods));
        title.Add(CreateMemberElements("Operators", type.Operators));
        title.Add(CreateMemberElements("Events", type.Events));
        return title;
    }

    [Pure]
    private static IEnumerable<XElement> CreateMemberElements(string name, IReadOnlyList<OutputNode> members)
    {
        if (members.Count == 0)
        {
            yield break;
        }

        var container = CreateTitleElement(name);

        container.Add(members
            .OrderBy(m => m.MenuName)
            .Select(CreateMemberTocElement));

        yield return container;
    }

    [Pure]
    private static XElement CreateTitleElement(string title) => CreateTocElement(title, null);

    [Pure]
    private static XElement CreateTopicElement(string topic) => CreateTocElement(null, topic);

    [Pure]
    private static XElement CreateMemberTocElement(OutputNode member) => CreateTocElement(member.MenuName, member.FileName);

    [Pure]
    private static XElement CreateTocElement(string? title, string? topic)
    {
        var element = new XElement("toc-element");
        if (title != null)
        {
            element.Add(new XAttribute("toc-title", title));
        }
        if (topic != null)
        {
            element.Add(new XAttribute("topic", topic));
        }
        return element;
    }
}