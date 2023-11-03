namespace MrKWatkins.DocGen.Model;

public sealed class TypeParameter : ModelNode
{
    public TypeParameter(System.Type type)
        : base(type.DisplayName())
    {
    }

    public new Type Parent => (Type)base.Parent;
}