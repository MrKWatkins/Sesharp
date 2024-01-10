using System.Reflection;
using MrKWatkins.DocGen.Markdown.Writing;
using MrKWatkins.DocGen.Model;
using Type = System.Type;

namespace MrKWatkins.DocGen.Markdown.Generation;

public sealed class TypeMarkdownGenerator : MarkdownGenerator<Model.Type>
{
    public TypeMarkdownGenerator(MemberLookup memberLookup, string outputDirectory)
        : base(memberLookup, outputDirectory)
    {
    }

    public override void Generate(Model.Type type)
    {
        var filePath = Path.Combine(OutputDirectory, type.FileName);
        using var writer = new MarkdownWriter(filePath);

        // TODO: Namespace.
        // TODO: Source code links.

        writer.WriteMainHeading($"{type.DisplayName} {type.Kind.Capitalize()}");

        writer.WriteSubHeading("Definition");

        WriteSection(writer, type.Documentation?.Summary);

        WriteSignature(writer, type);

        WriteTypeParameters(writer, type);

        // TODO: Inheritance.

        WriteMembers<ConstructorMarkdownGenerator, Constructor, MethodBase>(writer, type.Constructors);

        WriteMembers<FieldMarkdownGenerator, Field, FieldInfo>(writer, type.Fields);

        WriteMembers<PropertyMarkdownGenerator, Property, PropertyInfo>(writer, type.Properties);

        WriteMembers<MethodMarkdownGenerator, Method, MethodBase>(writer, type.Methods);

        WriteMembers<OperatorMarkdownGenerator, Operator, MethodBase>(writer, type.Operators);

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

        using var table = writer.Table();

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

    private void WriteMembers<TMemberGenerator, TMember, TMemberInfo>(MarkdownWriter writer, IReadOnlyList<TMember> members)
        where TMemberGenerator : MemberMarkdownGenerator<TMember, TMemberInfo>
        where TMember : DocumentableNode<TMemberInfo>
        where TMemberInfo : MemberInfo
    {
        if (members.Count == 0)
        {
            return;
        }

        var generator = (TMemberGenerator)Activator.CreateInstance(typeof(TMemberGenerator), MemberLookup, OutputDirectory)!;

        generator.Generate(members);
        generator.WriteTypeSection(writer, members);
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