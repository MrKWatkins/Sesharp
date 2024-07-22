using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Constructor(ConstructorInfo constructorInfo) : Function<ConstructorInfo>(constructorInfo);