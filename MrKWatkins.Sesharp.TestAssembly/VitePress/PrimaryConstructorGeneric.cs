namespace MrKWatkins.Sesharp.TestAssembly.VitePress;

/// <summary>
/// Represents a primary constructor generic class.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
/// <param name="value">The initial value.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class PrimaryConstructorGeneric<T>(T value)
{
    public T Value { get; } = value;
}