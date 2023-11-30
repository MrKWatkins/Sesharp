using System.Reflection;
using System.Text;

namespace MrKWatkins.DocGen.Model;

public sealed class Property : Member<PropertyInfo>
{
    private string? documentationKey;

    public Property(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
        Children.Add(propertyInfo.GetIndexParameters().Select(p => new Parameter(p)));
    }

    public IReadOnlyList<Parameter> IndexParameters => Children.OfType<Parameter>().ToList();    // Keep in declaration order.

    public override string DisplayName => IndexParameters.Any() ? $"{Name}[]" : Name;

    public override string DocumentationKey => documentationKey ??= BuildDocumentationKey();

    private string BuildDocumentationKey()
    {
        var sb = new StringBuilder("P:")
            .Append(Parent.Namespace.Name)
            .Append('.')
            .Append(Parent.MemberInfo.Name)
            .Append('.')
            .Append(Name);

        var parameters = IndexParameters;
        if (parameters.Count > 0)
        {
            sb.Append('(');

            foreach (var parameter in parameters)
            {
                if (parameter != parameters[0])
                {
                    sb.Append(',');
                }

                sb.Append(parameter.Type.FullName);
            }

            sb.Append(')');
        }

        return sb.ToString();
    }
}