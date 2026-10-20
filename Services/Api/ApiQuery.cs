namespace Phloem.Services.Api;

/// <summary>Builds "path?name=value&amp;…" for GET requests, skipping empty values and escaping the rest.</summary>
public static class ApiQuery
{
    /// <summary>Adds the non-empty values to the path.</summary>
    /// <param name="path">e.g. "api/dishes".</param>
    /// <param name="values">Name → value; null or empty values are left out.</param>
    /// <returns>e.g. "api/dishes?country=Ghana&amp;sort=Rating".</returns>
    public static string Build(string path, IReadOnlyDictionary<string, string?> values)
    {
        var parts = values
            .Where(v => !string.IsNullOrEmpty(v.Value))
            .Select(v => $"{Uri.EscapeDataString(v.Key)}={Uri.EscapeDataString(v.Value!)}")
            .ToList();

        return parts.Count == 0 ? path : $"{path}?{string.Join('&', parts)}";
    }
}
