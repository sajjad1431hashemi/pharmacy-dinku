using System.Text.Json.Serialization;

namespace PharmacyMiddleware.Models.Bale;

public sealed class BaleMessage
{
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [JsonPropertyName("from")]
    public BaleUser? From { get; set; }

    [JsonPropertyName("chat")]
    public BaleChat? Chat { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }  // Unix timestamp

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}