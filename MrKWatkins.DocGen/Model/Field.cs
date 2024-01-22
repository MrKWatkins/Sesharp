using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Field : Member<FieldInfo>
{
    public Field(FieldInfo fieldInfo)
        : base(fieldInfo)
    {
    }

    public Visibility Visibility => MemberInfo.GetVisibility();

    public bool IsConst => MemberInfo.IsConst();

    public bool IsReadOnly => MemberInfo.IsReadOnly();

    public bool IsStatic => MemberInfo.IsStatic;
}