using System.Text.Json.Serialization;

namespace PharmacyMiddleware.Models.Bale;

public sealed class BaleCallbackQuery
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public BaleUser? From { get; set; }

    [JsonPropertyName("message")]
    public BaleMessage? Message { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }   // مثل "refresh" یا "feedback"
}