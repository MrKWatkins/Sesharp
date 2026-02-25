namespace MrKWatkins.Sesharp.TestAssembly.Generics;

/// <summary>
/// A generic class with two type parameters.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GenericMultipleClass<TKey, TValue>
{
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public TKey? Key { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public TValue? Value { get; set; }
}