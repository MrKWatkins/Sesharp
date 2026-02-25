namespace MrKWatkins.Sesharp.TestAssembly.InheritDoc;

/// <summary>A class implementing <see cref="IInheritDocInterface" /> using inheritdoc.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class InheritDocImplementation : IInheritDocInterface
{
    /// <inheritdoc />
    public int GetValue() => 0;

    /// <inheritdoc />
    public void SetValue(int value) { }

    /// <inheritdoc />
    public int Property => 0;
}