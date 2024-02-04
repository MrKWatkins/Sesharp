using MrKWatkins.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Type : DocumentableNode<System.Type>
{
    public Type(System.Type type)
        : base(type)
    {
        if (type.IsGenericType)
        {
            Children.Add(type.GetGenericArguments().Select(t => new TypeParameter(t)));
        }
    }

    public override string DisplayName => MemberInfo.ToDisplayName();

    public string NonGenericName => MemberInfo.IsGenericType ? MemberInfo.Name[..^2] : MemberInfo.Name;

    public new Namespace Parent => (Namespace)base.Parent;

    public Namespace Namespace => Parent;

    public IReadOnlyList<TypeParameter> TypeParameters => Children.OfType<TypeParameter>().ToList();    // Keep in declaration order.

    public ConstructorGroup? ConstructorGroup => Children.FirstOfTypeOrDefault<ConstructorGroup>();

    public IReadOnlyList<Field> Fields => Children.OfType<Field>().ToList();

    public IReadOnlyList<Property> Properties => Children.OfType<Property>().ToList();

    public IReadOnlyList<OutputNode> Methods => Children.Where(c => c is Method or MethodGroup).OfType<OutputNode>().ToList();

    public IReadOnlyList<OutputNode> Operators => Children.Where(c => c is Operator or OperatorGroup).OfType<OutputNode>().ToList();

    public IReadOnlyList<Event> Events => Children.OfType<Event>().ToList();

    public string Kind
    {
        get
        {
            if (MemberInfo.IsEnum)
            {
                return "enum";
            }
            if (MemberInfo.IsValueType)
            {
                return "struct";
            }
            if (MemberInfo.IsRecord())
            {
                return "record";
            }
            if (MemberInfo.IsClass)
            {
                return "class";
            }
            if (MemberInfo.IsInterface)
            {
                return "interface";
            }

            throw new NotSupportedException($"Unsupported kind for type {Name}.");
        }
    }
}