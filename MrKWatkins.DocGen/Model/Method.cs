using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Method : Function<MethodInfo>
{
    public Method(MethodInfo method)
        : base(method)
    {
    }
}