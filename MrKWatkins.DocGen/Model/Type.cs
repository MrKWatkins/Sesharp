namespace MrKWatkins.DocGen.Model;

public sealed class Type : DocumentableNode<System.Type>
{
    public Type(System.Type type)
        : base(type)
    {
    }

    public override string DisplayName => MemberInfo.DisplayName();

    public override string DocumentationKey => $"T:{Namespace.Name}.{MemberInfo.Name}";

    public new Namespace Parent => (Namespace)base.Parent;

    public Namespace Namespace => Parent;

    public IEnumerable<TypeParameter> TypeParameters => Children.OfType<TypeParameter>();

    public IEnumerable<Constructor> Constructors => Children.OfType<Constructor>();

    public IEnumerable<Field> Fields => Children.OfType<Field>();

    public new IEnumerable<Property> Properties => Children.OfType<Property>();

    public IEnumerable<Method> Methods => Children.OfType<Method>();

    public IEnumerable<Event> Events => Children.OfType<Event>();

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