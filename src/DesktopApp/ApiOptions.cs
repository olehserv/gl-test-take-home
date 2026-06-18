namespace DesktopApp;

/// <summary>
/// Strongly-typed settings for reaching the IngestionApi, bound from the "Api"
/// configuration section.
/// </summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>Base address of the IngestionApi, e.g. "https://localhost:7296" (no trailing slash).</summary>
    public string BaseUrl { get; set; } = string.Empty;
}
