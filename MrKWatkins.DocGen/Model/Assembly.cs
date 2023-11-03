namespace MrKWatkins.DocGen.Model;

public sealed class Assembly : ModelNode
{
    public Assembly(System.Reflection.Assembly assembly)
        : base(assembly.GetName().Name ?? throw new ArgumentException("Value has no name.", nameof(assembly)))
    {
        ReflectionAssembly = assembly;
        Version = assembly.GetName().Version ?? throw new ArgumentException("Value has no version.", nameof(assembly));
    }

    public System.Reflection.Assembly ReflectionAssembly
    {
        get => Properties.GetOrThrow<System.Reflection.Assembly>(nameof(ReflectionAssembly));
        private init => Properties.Set(nameof(ReflectionAssembly), value);
    }

    public Version Version
    {
        get => Properties.GetOrThrow<Version>(nameof(Version));
        private init => Properties.Set(nameof(Version), value);
    }

    public IEnumerable<Namespace> Namespaces => Children.OfType<Namespace>();
}