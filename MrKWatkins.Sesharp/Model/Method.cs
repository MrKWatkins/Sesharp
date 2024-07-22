using System.Reflection;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Method(MethodInfo method) : Function<MethodInfo>(method)
{
    public Virtuality Virtuality => MemberInfo.GetVirtuality();
}