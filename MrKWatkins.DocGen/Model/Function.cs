using System.Reflection;
using System.Text;

namespace MrKWatkins.DocGen.Model;

public abstract class Function : Member<MethodBase>
{
    private string? displayName;

    protected Function(MethodBase method)
        : base(method)
    {
        if (method.IsGenericMethod)
        {
            Children.Add(method.GetGenericArguments().Select(t => new TypeParameter(t)));
        }
        Children.Add(method.GetParameters().Select(p => new Parameter(p)));
    }

    public IReadOnlyList<TypeParameter> TypeParameters => Children.OfType<TypeParameter>().ToList();    // Keep in declaration order.

    public IReadOnlyList<Parameter> Parameters => Children.OfType<Parameter>().ToList();    // Keep in declaration order.

    public override string DisplayName => displayName ??= BuildDisplayName();

    private string BuildDisplayName()
    {
        var sb = new StringBuilder(Name);

        var typeParameters = TypeParameters;
        if (typeParameters.Count > 0)
        {
            sb.Append('<');
            foreach (var typeParameter in typeParameters)
            {
                if (typeParameter != typeParameters[0])
                {
                    sb.Append(", ");
                }

                sb.Append(typeParameter.Name);
            }

            sb.Append('>');
        }

        sb.Append('(');

        var parameters = Parameters;
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

        sb.Append(')');
        return sb.ToString();
    }
}