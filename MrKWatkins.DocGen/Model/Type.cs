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

    public override string DisplayName => MemberInfo.DisplayName();

    public new Namespace Parent => (Namespace)base.Parent;

    public Namespace Namespace => Parent;

    public IReadOnlyList<TypeParameter> TypeParameters => Children.OfType<TypeParameter>().ToList();    // Keep in declaration order.

    public IReadOnlyList<Constructor> Constructors => Children.OfType<Constructor>().ToList();

    public IReadOnlyList<Field> Fields => Children.OfType<Field>().ToList();

    public new IReadOnlyList<Property> Properties => Children.OfType<Property>().ToList();

    public IReadOnlyList<Method> Methods => Children.OfType<Method>().ToList();

    public IReadOnlyList<Operator> Operators => Children.OfType<Operator>().ToList();

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