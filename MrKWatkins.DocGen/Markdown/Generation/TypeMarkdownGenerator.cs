using System.Reflection;
using Humanizer;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;
using Type = System.Type;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class TypeMarkdownGenerator : MarkdownGenerator
{
    public TypeMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    public override void Generate(OutputNode node)
    {
        if (node is not Model.Type type)
        {
            throw new ArgumentException($"Value must be a {nameof(Model.Type)}.", nameof(node));
        }

        using var writer = CreateWriter(node);

        // TODO: Namespace.
        // TODO: Source code links.

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");

        writer.WriteSubHeading("Definition");

        WriteSection(writer, type.Documentation?.Summary);

        WriteSignature(writer, type);

        WriteTypeParameters(writer, type);

        // TODO: Inheritance.

        WriteMembers<ConstructorMarkdownGenerator, Constructor, ConstructorInfo>(writer, type.ConstructorGroup);

        WriteMembers<FieldMarkdownGenerator, Field, FieldInfo>(writer, type.Fields);

        WriteMembers<PropertyMarkdownGenerator, Property, PropertyInfo>(writer, type.Properties);

        WriteMembers<MethodMarkdownGenerator, Method, MethodInfo>(writer, type.Methods);

        WriteMembers<OperatorMarkdownGenerator, Operator, MethodInfo>(writer, type.Operators);

        WriteMembers<EventMarkdownGenerator, Event, EventInfo>(writer, type.Events);
    }

    private void WriteTypeParameters(MarkdownWriter writer, Model.Type type)
    {
        var typeParameters = type.TypeParameters.ToList();
        if (typeParameters.Count == 0)
        {
            return;
        }

        writer.WriteSubSubHeading("Type Parameters");

        using var table = writer.Table("Name", "Description");

        foreach (var typeParameter in typeParameters)
        {
            table.NewRow();
            table.Write(typeParameter.Name);
            table.NewColumn();
            if (type.Documentation?.TypeParameters.TryGetValue(typeParameter.Name, out var summary) == true)
            {
                WriteSection(table, summary);
            }
        }
    }

    private void WriteMembers<TMemberGenerator, TMember, TMemberInfo>(MarkdownWriter writer, OutputNode? member)
        where TMemberGenerator : MemberMarkdownGenerator<TMember, TMemberInfo>
        where TMember : Member<TMemberInfo>
        where TMemberInfo : MemberInfo
    {
        if (member == null)
        {
            return;
        }

        WriteMembers<TMemberGenerator, TMember, TMemberInfo>(writer, [member]);
    }

    private void WriteMembers<TMemberGenerator, TMember, TMemberInfo>(MarkdownWriter writer, IReadOnlyList<OutputNode> members)
        where TMemberGenerator : MemberMarkdownGenerator<TMember, TMemberInfo>
        where TMember : Member<TMemberInfo>
        where TMemberInfo : MemberInfo
    {
        if (members.Count == 0)
        {
            return;
        }

        var generator = (TMemberGenerator)Activator.CreateInstance(typeof(TMemberGenerator), MemberLookup, OutputDirectory)!;

        generator.Generate(members);

        var allMembers = members
            .SelectMany(m => m switch
            {
                TMember member => [member],
                MemberGroup<TMember, TMemberInfo> group => group.Members,
                _ => throw new ArgumentException($"Value must only contain {typeof(TMember).Name} or groups of {typeof(TMember).Name.Pluralize()}.", nameof(members))
            })
            .ToList();

        generator.WriteTypeSection(writer, allMembers);
    }

    private static void WriteSignature(MarkdownWriter writer, Model.Type type)
    {
        using var code = writer.CodeBlock();
        code.Write("public ");
        if (type.MemberInfo.IsAbstract)
        {
            code.Write("abstract ");
        }
        if (type.MemberInfo.IsSealed)
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
            code.Write(type.MemberInfo.BaseType!.DisplayName());
        }

        var separator = hasBaseType ? ", " : " : ";
        foreach (var @interface in type.MemberInfo.GetInterfaces())
        {
            code.Write(separator);
            code.Write(@interface.DisplayName());
            separator = ", ";
        }

        WriteGenericTypeConstraints(code, type.MemberInfo.GetGenericArguments());
    }

    private static void WriteGenericTypeConstraints(ITextWriter code, [InstantHandle] IEnumerable<Type> genericArguments)
    {
        foreach (var genericArgument in genericArguments)
        {
            code.WriteLine();
            code.Write("   where ");
            code.Write(genericArgument.Name);

            var separator = " : ";
            foreach (var constraint in genericArgument.GetGenericParameterConstraints())
            {
                code.Write(separator);
                code.Write(constraint.DisplayName());
                separator = ", ";
            }

            var attributes = genericArgument.GenericParameterAttributes;
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                code.Write(separator);
                code.Write("new()");
            }
        }
    }
}