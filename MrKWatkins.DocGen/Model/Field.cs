using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Field : Member<FieldInfo>
{
    public Field(FieldInfo fieldInfo)
        : base(fieldInfo)
    {
    }
}