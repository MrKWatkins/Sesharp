using MrKWatkins.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class TypeParameter(System.Type type) : ModelNode(type.ToDisplayName())
{
    public new Type Parent => (Type)base.Parent;
}