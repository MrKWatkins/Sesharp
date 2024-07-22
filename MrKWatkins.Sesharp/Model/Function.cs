using System.Reflection;
using System.Text;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public abstract class Function<TMethodInfo> : Member<TMethodInfo>
    where TMethodInfo : MethodBase
{
    private string? memberName;

    protected Function(TMethodInfo method)
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

    public Accessibility Accessibility => MemberInfo.GetAccessibility();

    public bool IsStatic => MemberInfo.IsStatic;

    public string TitleName
    {
        get
        {
            var sb = new StringBuilder(Type.NonGenericName);
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

                    sb.Append(parameter.Type.ToDisplayName());
                }
            }

            sb.Append(')');
            return sb.ToString();
        }
    }

    public override string MemberName => memberName ??= BuildMemberName();

    private string BuildMemberName()
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

                sb.Append(parameter.Type.ToDisplayName());
            }
        }

        sb.Append(')');
        return sb.ToString();
    }
}