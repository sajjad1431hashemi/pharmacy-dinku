using System.Text.Json.Serialization;

namespace PharmacyMiddleware.Models.Bale;

public sealed class BaleChat
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }   // "private", "group", ...
}