using System.Reflection;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Parameter(ParameterInfo parameter) : ModelNode(parameter.Name ?? "")
{
    public ParameterInfo ParameterInfo { get; } = parameter;

    public System.Type Type => ParameterInfo.ParameterType.IsByRef ? ParameterInfo.ParameterType.GetElementType()! : ParameterInfo.ParameterType;

    public ParameterKind Kind => ParameterInfo.GetKind();

    public bool HasDefaultValue => ParameterInfo.HasDefaultValue;

    public object? DefaultValue => ParameterInfo.DefaultValue;
}