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
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var typeNode = new Type(type);

        typeNode.Children.Add(
            type.GetConstructors(bindingFlags)
                .Where(IsPublicOrProtected)
                .Select(c => new Constructor(c))
                .OrderBy(f => f.DisplayName));

        typeNode.Children.Add(
            type.GetFields(bindingFlags)
                .Where(IsPublicOrProtected)
                .Select(f => new Field(f))
                .OrderBy(f => f.DisplayName));

        typeNode.Children.Add(
            type.GetProperties(bindingFlags)
                .Where(IsPublicOrProtected)
                .Select(p => new Property(p))
                .OrderBy(p => p.DisplayName));

        typeNode.Children.Add(
            type.GetMethods(bindingFlags)
                .Where(m => IsPublicOrProtected(m) && !IsPropertyMethod(m) && !IsOperatorMethod(m))
                .Select(m => new Method(m))
                .OrderBy(m => m.DisplayName));

        typeNode.Children.Add(
            type.GetMethods(bindingFlags)
                .Where(m => IsPublicOrProtected(m) && IsOperatorMethod(m))
                .Select(m => new Operator(m))
                .OrderBy(m => m.DisplayName));

        typeNode.Children.Add(
            type.GetEvents(bindingFlags)
                .Where(IsPublicOrProtected)
                .Select(e => new Event(e))
                .OrderBy(e => e.DisplayName));

        return typeNode;
    }

    [Pure]
    private static bool IsPublicOrProtected(FieldInfo field) => field.IsPublic || field.IsFamily;

    [Pure]
    private static bool IsPublicOrProtected(MethodBase method) => method.IsPublic || method.IsFamily;

    [Pure]
    private static bool IsPublicOrProtected(PropertyInfo property) =>
        (property.GetMethod != null && IsPublicOrProtected(property.GetMethod)) ||
        (property.SetMethod != null && IsPublicOrProtected(property.SetMethod));

    [Pure]
    private static bool IsPublicOrProtected(EventInfo @event) =>
        (@event.AddMethod != null && IsPublicOrProtected(@event.AddMethod)) ||
        (@event.RemoveMethod != null && IsPublicOrProtected(@event.RemoveMethod)) ||
        (@event.RaiseMethod != null && IsPublicOrProtected(@event.RaiseMethod));

    [Pure]
    private static bool IsPropertyMethod(MethodInfo method) =>
        method.Name.StartsWith("get_", StringComparison.Ordinal) ||
        method.Name.StartsWith("set_", StringComparison.Ordinal);

    [Pure]
    private static bool IsOperatorMethod(MethodInfo method) =>
        method.Name.StartsWith("op_", StringComparison.Ordinal);
}