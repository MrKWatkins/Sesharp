namespace MrKWatkins.Sesharp.TestAssembly.Generics;

/// <summary>
/// A generic class with a single type parameter.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GenericClass<T>
{
    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    public T? Item { get; set; }
}