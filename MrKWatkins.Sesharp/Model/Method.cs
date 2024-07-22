using System.Reflection;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Method : Function<MethodInfo>
{
    public Method(MethodInfo method)
        : base(method)
    {
    }

    public Virtuality Virtuality => MemberInfo.GetVirtuality();
}