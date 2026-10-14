using System.Text.Json.Serialization;

namespace PharmacyMiddleware.Models.Bale;

public sealed class BaleInlineKeyboardMarkup
{
    [JsonPropertyName("inline_keyboard")]
    public List<List<BaleInlineKeyboardButton>> InlineKeyboard { get; set; } = new();
}