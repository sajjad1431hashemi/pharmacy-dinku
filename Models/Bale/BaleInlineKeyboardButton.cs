using System.Text.Json.Serialization;

namespace PharmacyMiddleware.Models.Bale;

public sealed class BaleInlineKeyboardButton
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("callback_data")]
    public string? CallbackData { get; set; }
}