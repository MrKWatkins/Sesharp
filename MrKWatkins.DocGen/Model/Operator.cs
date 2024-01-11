using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Operator : Function<MethodInfo>
{
    public Operator(MethodInfo methodInfo)
        : base(methodInfo)
    {
    }

    public override string DisplayName => Name[3..];
}