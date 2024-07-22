using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Operator(MethodInfo methodInfo) : Function<MethodInfo>(methodInfo)
{
    public override string DisplayName => Name[3..];
}