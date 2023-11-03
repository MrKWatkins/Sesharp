using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Method : Function<MethodInfo>
{
    public Method(MethodInfo methodInfo)
        : base(methodInfo)
    {
    }

    // TODO: Parameters.
    public override string DocumentationKey => $"M:{Parent.Namespace.Name}.{Parent.MemberInfo.Name}.{Name}";
}