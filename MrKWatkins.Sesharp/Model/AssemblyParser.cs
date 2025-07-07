using System.Reflection;
using System.Runtime.CompilerServices;
using MrKWatkins.Sesharp.XmlDocumentation;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public static class AssemblyParser
{
    private const BindingFlags Binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Pure]
    public static AssemblyDetails Parse(Assembly assembly, Documentation documentation)
    {
        var assemblyNode = new AssemblyDetails(assembly);

        foreach (var group in assembly.GetExportedTypes().Where(t => t.IsPublic).GroupBy(t => t.Namespace ?? "global").OrderBy(g => g.Key))
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

        AddConstructors(type, typeNode);

        AddFields(type, typeNode);

        AddProperties(type, typeNode);

        AddMethods(type, typeNode);

        AddOperators(type, typeNode);

        AddEvents(type, typeNode);

        return typeNode;
    }

    private static void AddConstructors(System.Type type, Type typeNode)
    {
        var constructors =
            type.GetConstructors(Binding)
                .Where(c => IsNotCompilerGenerated(c) && c.IsPublicOrProtected())
                .Select(c => new Constructor(c))
                .OrderBy(f => f.DisplayName)
                .ToList();

        if (constructors.Any())
        {
            typeNode.Children.Add(new ConstructorGroup(constructors));
        }
    }

    private static void AddFields(System.Type type, Type typeNode)
    {
        typeNode.Children.Add(
            type.GetFields(Binding)
                .Where(f => IsNotCompilerGenerated(f) &&
                            type.IsEnum && f is { IsPublic: true, IsStatic: true } ||
                            !type.IsEnum && f.IsPublicOrProtected())
                .Select(f => new Field(f))
                .OrderBy(f => f.DisplayName));
    }

    private static void AddProperties(System.Type type, Type typeNode)
    {
        typeNode.Children.Add(
            type.GetProperties(Binding)
                .Where(p => IsNotCompilerGenerated(p) && p.IsPublicOrProtected())
                .Select(p => new Property(p))
                .OrderBy(p => p.DisplayName));
    }

    private static void AddMethods(System.Type type, Type typeNode)
    {
        typeNode.Children.Add(
            type.GetMethods(Binding)
                .Where(m => IsNotCompilerGenerated(m) && m.IsPublicOrProtected() && !IsPropertyMethod(m) && !IsOperatorMethod(m))
                .GroupBy(m => m.Name) // TODO: Remove generic parameters?
                .Select(g => g.Count() == 1
                    ? (OutputNode)new Method(g.First())
                    : new MethodGroup(g.Key, g.Select(m => new Method(m))))
                .OrderBy(m => m.DisplayName));
    }

    private static void AddOperators(System.Type type, Type typeNode)
    {
        typeNode.Children.Add(
            type.GetMethods(Binding)
                .Where(m => IsNotCompilerGenerated(m) && m.IsPublicOrProtected() && IsOperatorMethod(m))
                .GroupBy(m => m.Name)
                .Select(g => g.Count() == 1
                    ? (OutputNode)new Operator(g.First())
                    : new OperatorGroup(g.Key, g.Select(m => new Operator(m))))
                .OrderBy(m => m.DisplayName));
    }

    private static void AddEvents(System.Type type, Type typeNode)
    {
        typeNode.Children.Add(
            type.GetEvents(Binding)
                .Where(e => IsNotCompilerGenerated(e) && e.IsPublicOrProtected())
                .Select(e => new Event(e))
                .OrderBy(e => e.DisplayName));
    }

    [Pure]
    private static bool IsPropertyMethod(MethodInfo method) =>
        method.Name.StartsWith("get_", StringComparison.Ordinal) ||
        method.Name.StartsWith("set_", StringComparison.Ordinal);

    [Pure]
    private static bool IsOperatorMethod(MethodInfo method) =>
        method.Name.StartsWith("op_", StringComparison.Ordinal);

    [Pure]
    private static bool IsNotCompilerGenerated(MemberInfo member) => member.GetCustomAttribute<CompilerGeneratedAttribute>() == null;
}