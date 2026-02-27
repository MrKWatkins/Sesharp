namespace MrKWatkins.Sesharp.TestAssembly.MkDocs;

/// <summary>
/// An interface for testing MkDocs documentation generation. Implemented by <see cref="MkDocsClass"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public interface IMkDocsInterface
{
    /// <summary>
    /// Gets a value. Returns a <see cref="MkDocsClass"/> instance.
    /// </summary>
    MkDocsClass GetValue();
}
