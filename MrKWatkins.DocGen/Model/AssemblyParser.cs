using System.Reflection;
using MrKWatkins.DocGen.XmlDocumentation;

namespace MrKWatkins.DocGen.Model;

public static class AssemblyParser
{
    [Pure]
    public static Assembly Parse(System.Reflection.Assembly assembly, Documentation documentation)
    {
        var assemblyNode = new Assembly(assembly);

        foreach (var group in assembly.GetTypes().Where(t => t.IsPublic).GroupBy(t => t.Namespace ?? "global").OrderBy(g => g.Key))
        {
            var @namespace = new Namespace(group.Key);

            @namespace.Children.Add(group.Select(Parse).OrderBy(t => t.Name));

            assemblyNode.Children.Add(@namespace);
        }

        new DocumentationListener().Listen(documentation, assemblyNode);

        return assemblyNode;
    }

    [Pure]
    private static Type Parse(System.Type type)
    {
        var typeNode = new Type(type);

        if (type.IsGenericType)
        {
            typeNode.Children.Add(type.GetGenericArguments().Select(t => new TypeParameter(t)));
        }

        typeNode.Children.Add(
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(p => new Property(p))
                .OrderBy(p => p.Name));

        return typeNode;
    }
}