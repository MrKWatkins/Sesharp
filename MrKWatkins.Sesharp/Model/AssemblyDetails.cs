namespace MrKWatkins.Sesharp.Model;

// TODO: Group members by overload groups - make it easier for page per group.
public sealed class AssemblyDetails : ModelNode
{
    public AssemblyDetails(System.Reflection.Assembly assembly)
        : base(assembly.GetName().Name ?? throw new ArgumentException("Value has no name.", nameof(assembly)))
    {
        ReflectionAssembly = assembly;
        Version = assembly.GetName().Version ?? throw new ArgumentException("Value has no version.", nameof(assembly));
    }

    public System.Reflection.Assembly ReflectionAssembly { get; }

    public Version Version { get; }

    public IEnumerable<Namespace> Namespaces => Children.OfType<Namespace>();
}