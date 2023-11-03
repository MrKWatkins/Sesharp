using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Field : Member<FieldInfo>
{
    public Field(FieldInfo fieldInfo)
        : base(fieldInfo)
    {
    }

    public override string DocumentationKey => $"F:{Parent.Namespace.Name}.{Parent.MemberInfo.Name}.{Name}";
}