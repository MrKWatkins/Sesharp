namespace MrKWatkins.Sesharp.TestAssembly.MkDocs;

/// <summary>
/// A generic class for testing MkDocs documentation generation. Works with <see cref="MkDocsClass"/>.
/// </summary>
/// <typeparam name="T">The type parameter.</typeparam>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class MkDocsGenericClass<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MkDocsGenericClass{T}"/> class.
    /// </summary>
    /// <param name="source">The source <see cref="MkDocsClass"/>.</param>
    public MkDocsGenericClass(MkDocsClass source)
    {
        Source = source;
    }

    /// <summary>
    /// The source <see cref="MkDocsClass"/>.
    /// </summary>
    public MkDocsClass Source { get; }

    /// <summary>
    /// Processes a <see cref="MkDocsClass"/> and returns a <see cref="MkDocsStruct"/>.
    /// </summary>
    /// <param name="input">The <see cref="MkDocsClass"/> to process.</param>
    /// <returns>A <see cref="MkDocsStruct"/> result.</returns>
    public MkDocsStruct Process(MkDocsClass input) => input.ToStruct();
}
