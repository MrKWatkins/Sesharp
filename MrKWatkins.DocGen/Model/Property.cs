using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Property : Member<PropertyInfo>
{
    public Property(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
    }

    public override string DocumentationKey => $"P:{Parent.Namespace.Name}.{Parent.MemberInfo.Name}.{Name}";
}