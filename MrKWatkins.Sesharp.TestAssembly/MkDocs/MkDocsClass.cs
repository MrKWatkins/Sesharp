namespace MrKWatkins.Sesharp.TestAssembly.MkDocs;

/// <summary>
/// A class for testing MkDocs documentation generation. Implements <see cref="IMkDocsInterface"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class MkDocsClass : IMkDocsInterface
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MkDocsClass"/> class.
    /// </summary>
    /// <param name="value">The value. Used by <see cref="MkDocsGenericClass{T}"/>.</param>
    public MkDocsClass(int value)
    {
        Number = value;
        Generic = new MkDocsGenericClass<int>(this);
    }

    /// <summary>
    /// The integer value.
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// A generic property of type <see cref="MkDocsGenericClass{T}"/>.
    /// </summary>
    public MkDocsGenericClass<int> Generic { get; }

    /// <inheritdoc />
    public MkDocsClass GetValue() => this;

    /// <summary>
    /// Creates a <see cref="MkDocsStruct"/> from this instance.
    /// </summary>
    /// <returns>A new <see cref="MkDocsStruct"/>.</returns>
    public MkDocsStruct ToStruct() => new(Number);
}
