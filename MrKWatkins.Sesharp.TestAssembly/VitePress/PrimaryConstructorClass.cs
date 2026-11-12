namespace MrKWatkins.Sesharp.TestAssembly.VitePress;

/// <summary>
/// Represents a primary constructor class.
/// </summary>
/// <remarks>
/// Values are recorded without validation.
/// </remarks>
/// <param name="value">The initial value.</param>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class PrimaryConstructorClass(int value)
{
    /// <summary>
    /// Creates the default instance using zero.
    /// </summary>
    public PrimaryConstructorClass()
        : this(0)
    {
    }

    public int Value { get; } = value;
}