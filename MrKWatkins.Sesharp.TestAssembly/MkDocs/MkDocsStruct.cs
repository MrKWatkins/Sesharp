namespace MrKWatkins.Sesharp.TestAssembly.MkDocs;

/// <summary>
/// A struct for testing MkDocs documentation generation. Created from a <see cref="MkDocsClass"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public readonly struct MkDocsStruct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MkDocsStruct"/> struct.
    /// </summary>
    /// <param name="value">The value.</param>
    public MkDocsStruct(int value)
    {
        Value = value;
    }

    /// <summary>
    /// The integer value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Converts this struct back to a <see cref="MkDocsClass"/>.
    /// </summary>
    /// <returns>A new <see cref="MkDocsClass"/>.</returns>
    public MkDocsClass ToClass() => new(Value);
}
