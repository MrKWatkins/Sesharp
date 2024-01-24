using System.Reflection;
using System.Text;

namespace MrKWatkins.DocGen.Model;

public sealed class Property : Member<PropertyInfo>
{
    private string? memberName;

    public Property(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
        Children.Add(propertyInfo.GetIndexParameters().Select(p => new Parameter(p)));
    }

    public IReadOnlyList<Parameter> IndexParameters => Children.OfType<Parameter>().ToList();    // Keep in declaration order.

    public override string MenuName => IndexParameters.Any() ? $"{Name}[]" : Name;

    public override string MemberName => memberName ??= BuildMemberName();

    private string BuildMemberName()
    {
        var parameters = IndexParameters;
        if (parameters.Count == 0)
        {
            return DisplayName;
        }

        var sb = new StringBuilder(DisplayName);
        sb.Append('[');

        if (parameters.Count > 0)
        {
            foreach (var parameter in parameters)
            {
                if (parameter != parameters[0])
                {
                    sb.Append(", ");
                }

                sb.Append(parameter.Type.DisplayName());
            }
        }

        sb.Append(']');
        return sb.ToString();
    }

    public bool IsRequired => MemberInfo.IsRequired();

    public MethodInfo? Getter => MemberInfo.GetMethod?.IsPublicOrProtected() == true ? MemberInfo.GetMethod : null;

    public MethodInfo? Setter => MemberInfo.SetMethod?.IsPublicOrProtected() == true ? MemberInfo.SetMethod : null;

    public bool HasInitSetter => MemberInfo.HasInitSetter();

    public Visibility Visibility => MemberInfo.GetVisibility() ?? throw new InvalidOperationException("Property is not visible.");

    public Virtuality? Virtuality => MemberInfo.GetVirtuality();

    public bool IsStatic => MemberInfo.IsStatic();
}