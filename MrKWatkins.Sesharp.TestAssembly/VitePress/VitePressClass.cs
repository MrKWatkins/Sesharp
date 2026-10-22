namespace MrKWatkins.Sesharp.TestAssembly.VitePress;

/// <summary>
/// Literal &lt;tag&gt; &amp; {{literal}} {value} | text. Links to <see cref="Nested"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class VitePressClass<T>
{
    /// <summary>Creates an instance.</summary>
    public VitePressClass() { }

    /// <summary>Creates an instance with a value.</summary>
    /// <param name="value">The value.</param>
    public VitePressClass(T value) { }

    /// <summary>Reads <c>a|b</c> &amp; {{literal}} {value} | text.</summary>
    /// <param name="value">A nullable value.</param>
    /// <returns>The supplied value.</returns>
    public string? Read(string? value) => value;

    /// <summary>Reads a sequence.</summary>
    /// <param name="values">The values.</param>
    /// <returns>The first value.</returns>
    public T? Read(IEnumerable<T> values) => values.FirstOrDefault();

    /// <summary>A nested type in a generic type.</summary>
    public sealed class Nested;
}

/// <summary>A second parent to test nested type paths.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class AnotherClass
{
    /// <summary>A nested type with the same name as one in a different parent.</summary>
    public sealed class Nested;
}

/// <summary>An operator fixture.</summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct VitePressValue
{
    /// <summary>Combines <c>a|b</c>.</summary>
    public static VitePressValue operator |(VitePressValue left, VitePressValue right) => left;

    /// <summary>Combines an integer value.</summary>
    public static VitePressValue operator |(VitePressValue left, int right) => left;
}