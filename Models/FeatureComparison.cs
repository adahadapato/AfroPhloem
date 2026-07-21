namespace AfroPhloem.Models;

/// <summary>
/// Static content model for the About page's "what we do differently" comparison cards.
/// </summary>
public class FeatureComparison
{
    public string Icon { get; set; } = "🔥";
    public string Title { get; set; } = string.Empty;
    public string ExistingPlatformText { get; set; } = string.Empty;
    public string AfroPhloemText { get; set; } = string.Empty;
    public string ValueAdded { get; set; } = string.Empty;
}
