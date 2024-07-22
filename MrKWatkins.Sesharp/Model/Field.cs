using System.Reflection;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Field(FieldInfo fieldInfo) : Member<FieldInfo>(fieldInfo)
{
    public Accessibility Accessibility => MemberInfo.GetAccessibility();

    public bool IsConst => MemberInfo.IsConst();

    public bool IsReadOnly => MemberInfo.IsReadOnly();

    public bool IsStatic => MemberInfo.IsStatic;
}