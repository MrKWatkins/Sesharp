using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Property : Member<PropertyInfo>
{
    public Property(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
        Children.Add(propertyInfo.GetIndexParameters().Select(p => new Parameter(p)));
    }

    public IReadOnlyList<Parameter> IndexParameters => Children.OfType<Parameter>().ToList();    // Keep in declaration order.

    public override string MemberName => IndexParameters.Any() ? $"{Name}[]" : Name;
}