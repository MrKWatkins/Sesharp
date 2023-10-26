using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Method : Function<MethodInfo>
{
    public Method(MethodInfo methodInfo)
        : base(methodInfo)
    {
    }

    public override string DocumentationKey => throw new NotSupportedException();
}