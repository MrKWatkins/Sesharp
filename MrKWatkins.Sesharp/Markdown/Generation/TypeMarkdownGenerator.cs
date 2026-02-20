using System.Reflection;
using Humanizer;
using MrKWatkins.Sesharp.Markdown.Writing;
using MrKWatkins.Sesharp.Model;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Markdown.Generation;

public sealed class TypeMarkdownGenerator(IFileSystem fileSystem, MemberLookup memberLookup, string outputDirectory) : MarkdownGenerator(fileSystem, memberLookup, outputDirectory)
{
    public override void Generate(OutputNode node)
    {
        if (node is not Model.Type type)
        {
            throw new ArgumentException($"Value must be a {nameof(Model.Type)}.", nameof(node));
        }

        using var writer = CreateWriter(node);

        // TODO: Namespace.
        // TODO: Assembly.
        // TODO: Source code links.

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");

        writer.WriteSubHeading("Definition");

        WriteSection(writer, type.Documentation?.Summary);

        WriteSignature(writer, type);

        WriteTypeParameters(writer, type, type.TypeParameters);

        // TODO: Inheritance.

        WriteMembers<ConstructorMarkdownGenerator, Constructor, ConstructorInfo>(writer, type.ConstructorGroup, c => c.TitleName);

        WriteMembers<FieldMarkdownGenerator, Field, FieldInfo>(writer, type.Fields);

        WriteMembers<PropertyMarkdownGenerator, Property, PropertyInfo>(writer, type.Properties);

        WriteMembers<MethodMarkdownGenerator, Method, MethodInfo>(writer, type.Methods);

        WriteMembers<OperatorMarkdownGenerator, Operator, MethodInfo>(writer, type.Operators);

        WriteMembers<EventMarkdownGenerator, Event, EventInfo>(writer, type.Events);

        WriteSeeAlsos(writer, type.Documentation?.SeeAlsos);
    }

    private void WriteMembers<TMemberGenerator, TMember, TMemberInfo>(MarkdownWriter writer, OutputNode? member, Func<TMember, string>? getMemberName = null)
        where TMemberGenerator : MemberMarkdownGenerator<TMember, TMemberInfo>
        where TMember : Member<TMemberInfo>
        where TMemberInfo : MemberInfo
    {
        if (member == null)
        {
            return;
        }

        WriteMembers<TMemberGenerator, TMember, TMemberInfo>(writer, [member], getMemberName);
    }

    private void WriteMembers<TMemberGenerator, TMember, TMemberInfo>(MarkdownWriter writer, IReadOnlyList<OutputNode> members, Func<TMember, string>? getMemberName = null)
        where TMemberGenerator : MemberMarkdownGenerator<TMember, TMemberInfo>
        where TMember : Member<TMemberInfo>
        where TMemberInfo : MemberInfo
    {
        if (members.Count == 0)
        {
            return;
        }

        var generator = (TMemberGenerator)Activator.CreateInstance(typeof(TMemberGenerator), FileSystem, MemberLookup, OutputDirectory)!;

        generator.Generate(members);

        var allMembers = members
            .SelectMany(m => m switch
            {
                TMember member => [member],
                MemberGroup<TMember, TMemberInfo> group => group.Members,
                _ => throw new ArgumentException($"Value must only contain {typeof(TMember).Name} or groups of {typeof(TMember).Name.Pluralize()}.", nameof(members))
            })
            .ToList();

        // Restore the type page's file so WriteMemberTable generates links relative to it.
        generator.CurrentNodeFile = CurrentNodeFile;

        generator.WriteMemberTable(writer, allMembers, getMemberName: getMemberName);
    }

    private static void WriteSignature(MarkdownWriter writer, Model.Type type)
    {
        using var code = writer.CodeBlock();
        code.Write("public ");

        if (type.MemberInfo.IsEnum)
        {
            code.Write("enum ");
            code.Write(type.DisplayName);
            return;
        }

        if (type.MemberInfo.IsStatic())
        {
            code.Write("static ");
        }
        else if (type.MemberInfo.IsAbstract)
        {
            code.Write("abstract ");
        }
        else if (type.MemberInfo.IsSealed)
        {
            code.Write("sealed ");
        }

        code.Write(type.Kind);
        code.Write(" ");
        code.Write(type.DisplayName);

        var hasBaseType = type.MemberInfo.IsClass && type.MemberInfo.BaseType != typeof(object);
        if (hasBaseType)
        {
            code.Write(" : ");
            code.Write(type.MemberInfo.BaseType!.ToDisplayName());
        }

        var separator = hasBaseType ? ", " : " : ";
        foreach (var @interface in type.MemberInfo.GetInterfaces())
        {
            code.Write(separator);
            code.Write(@interface.ToDisplayName());
            separator = ", ";
        }

        WriteSignatureTypeConstraints(code, type.MemberInfo.GetGenericArguments());
    }
}