using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Constructor : Function<ConstructorInfo>
{
    public Constructor(ConstructorInfo constructorInfo)
        : base(constructorInfo)
    {
    }
}