using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Parameter : ModelNode
{
    public Parameter(ParameterInfo parameter)
        : base(parameter.Name ?? "")
    {
        ParameterInfo = parameter;
    }

    public ParameterInfo ParameterInfo { get; }

    public System.Type Type => ParameterInfo.ParameterType;

    public bool HasDefaultValue => ParameterInfo.HasDefaultValue;

    public object? DefaultValue => ParameterInfo.DefaultValue;
}