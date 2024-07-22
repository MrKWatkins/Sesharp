namespace MrKWatkins.Sesharp.Model;

// TODO: Group members by overload groups - make it easier for page per group.
public sealed class AssemblyDetails(System.Reflection.Assembly assembly) : ModelNode(assembly.GetName().Name ?? throw new ArgumentException("Value has no name.", nameof(assembly)))
{
    public System.Reflection.Assembly ReflectionAssembly { get; } = assembly;

    public Version Version { get; } = assembly.GetName().Version ?? throw new ArgumentException("Value has no version.", nameof(assembly));

    public IEnumerable<Namespace> Namespaces => Children.OfType<Namespace>();
}