using System.Reflection;
using System.Text;

namespace MrKWatkins.DocGen.Model;

public abstract class Function : Member<MethodBase>
{
    private string? displayName;
    private string? documentationKey;

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

    public sealed override string DocumentationKey => documentationKey ??= BuildDocumentationKey();

    private string BuildDocumentationKey()
    {
        var sb = new StringBuilder("M:")
            .Append(Parent.Namespace.Name)
            .Append('.')
            .Append(Parent.MemberInfo.Name)
            .Append('.')
            .Append(Name);

        var typeParameters = TypeParameters.Count;
        if (typeParameters > 0)
        {
            sb.Append("``").Append(typeParameters);
        }

        var parameters = Parameters;
        if (parameters.Count > 0)
        {
            sb.Append('(');

            foreach (var parameter in parameters)
            {
                if (parameter != parameters[0])
                {
                    sb.Append(',');
                }

                sb.Append(GetParameterKey(parameter.Type));
            }

            sb.Append(')');
        }

        return sb.ToString();
    }

    [Pure]
    private string GetParameterKey(System.Type parameterType)
    {
        if (parameterType.IsGenericType)
        {
            var genericTypeKey = parameterType.GetGenericTypeDefinition().FullName![..^2];
            var parameterKeys = string.Join(',', parameterType.GetGenericArguments().Select(GetParameterKey));
            return $"{genericTypeKey}{{{parameterKeys}}}";
        }

        if (parameterType.IsArray)
        {
            return $"{GetParameterKey(parameterType.GetElementType()!)}[]";
        }

        if (!parameterType.IsGenericParameter)
        {
            return parameterType.FullName!;
        }

        var functionParameterIndex = TypeParameters.IndexOf(t => t.Name == parameterType.Name);
        if (functionParameterIndex != -1)
        {
            return $"``{functionParameterIndex}";
        }

        var typeParameterIndex = Parent.TypeParameters.IndexOf(t => t.Name == parameterType.Name);
        if (typeParameterIndex != -1)
        {
            return $"`{typeParameterIndex}";
        }

        throw new InvalidOperationException("Could not resolve generic type parameter.");
    }
}