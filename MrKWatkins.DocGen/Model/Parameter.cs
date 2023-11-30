using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Parameter : ModelNode
{
    public Parameter(ParameterInfo parameter)
        : base(parameter.Name ?? "")
    {
        Type = parameter.ParameterType;
    }

    public new Function Parent => (Function)base.Parent;

    public System.Type Type { get; }
}