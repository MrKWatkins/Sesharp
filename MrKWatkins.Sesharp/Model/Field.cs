using System.Reflection;
using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Field : Member<FieldInfo>
{
    public Field(FieldInfo fieldInfo)
        : base(fieldInfo)
    {
    }

    public Accessibility Accessibility => MemberInfo.GetAccessibility();

    public bool IsConst => MemberInfo.IsConst();

    public bool IsReadOnly => MemberInfo.IsReadOnly();

    public bool IsStatic => MemberInfo.IsStatic;
}