using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Operator : Function<MethodInfo>
{
    public Operator(MethodInfo methodInfo)
        : base(methodInfo)
    {
    }

    public override string DisplayName => Name[3..];
}