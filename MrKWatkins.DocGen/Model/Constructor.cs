using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Constructor : Function<ConstructorInfo>
{
    public Constructor(ConstructorInfo constructorInfo)
        : base(constructorInfo)
    {
    }

    public override string DocumentationKey => throw new NotSupportedException();
}