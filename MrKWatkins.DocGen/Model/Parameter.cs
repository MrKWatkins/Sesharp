using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Parameter : ModelNode
{
    public Parameter(ParameterInfo parameter)
        : base(parameter.Name ?? "")
    {
        Type = parameter.ParameterType;
    }

    public System.Type Type { get; }
}