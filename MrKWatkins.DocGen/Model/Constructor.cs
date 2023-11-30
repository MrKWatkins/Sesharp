using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Constructor : Function
{
    public Constructor(ConstructorInfo constructorInfo)
        : base(constructorInfo)
    {
    }

    public new ConstructorInfo MemberInfo => (ConstructorInfo)base.MemberInfo;
}