using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phloem.Services.Api;

/// <summary>JSON settings matching the API: camelCase names, enums as words ("UnitedKingdom", "Asap").</summary>
public static class ApiJson
{
    /// <summary>Use these for every request and response.</summary>
    public static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
