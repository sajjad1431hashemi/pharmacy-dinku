using System.Text.Json.Serialization;

namespace PharmacyMiddleware.Models.Bale;

public sealed class BaleUser
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }
}