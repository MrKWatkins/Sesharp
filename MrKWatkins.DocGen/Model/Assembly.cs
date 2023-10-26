namespace MrKWatkins.DocGen.Model;

public sealed class Assembly : ModelNode
{
    public Assembly(System.Reflection.Assembly assembly)
        : base(assembly.GetName().Name ?? throw new ArgumentException("Value has no name.", nameof(assembly)))
    {
        Version = assembly.GetName().Version ?? throw new ArgumentException("Value has no version.", nameof(assembly));
    }

    public Version Version
    {
        get => Properties.GetOrThrow<Version>(nameof(Version));
        private init => Properties.Set(nameof(Version), value);
    }

    public IEnumerable<Namespace> Namespaces => Children.OfType<Namespace>();
}