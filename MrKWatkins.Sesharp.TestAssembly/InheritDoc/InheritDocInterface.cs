namespace MrKWatkins.Sesharp.TestAssembly.InheritDoc;

/// <summary>An interface with documented members.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public interface IInheritDocInterface
{
    /// <summary>Gets a value.</summary>
    /// <returns>The value.</returns>
    int GetValue();

    /// <summary>Sets a value.</summary>
    /// <param name="value">The value to set.</param>
    void SetValue(int value);

    /// <summary>A documented property.</summary>
    int Property { get; }
}