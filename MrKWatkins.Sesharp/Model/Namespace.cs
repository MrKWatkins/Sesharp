namespace MrKWatkins.Sesharp.Model;

public sealed class Namespace(string name) : ModelNode(name)
{
    public new AssemblyDetails Parent => (AssemblyDetails)base.Parent;

    public IEnumerable<Type> Types => Children.OfType<Type>();
}