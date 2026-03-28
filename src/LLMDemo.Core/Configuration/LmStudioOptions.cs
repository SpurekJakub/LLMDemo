namespace LLMDemo.Core.Configuration;

/// <summary>
/// Configuration options for connecting to LM Studio.
/// Bind from appsettings.json section "LmStudio".
/// </summary>
public sealed class LmStudioOptions
{
    public const string SectionName = "LmStudio";

    /// <summary>Base URL of the LM Studio API (OpenAI-compatible endpoint).</summary>
    public Uri Endpoint { get; set; } = new("http://localhost:1234/v1");

    /// <summary>Default model identifier to use when none is specified.</summary>
    public string? DefaultModel { get; set; }
}
