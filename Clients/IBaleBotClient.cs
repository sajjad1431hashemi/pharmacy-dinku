using PharmacyMiddleware.Models.Bale;

namespace PharmacyMiddleware.Clients;

public interface IBaleBotClient
{
    Task<long> SendMessageAsync(
        long chatId,
        string text,
        BaleInlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default);

    Task<bool> DeleteMessageAsync(
        long chatId,
        long messageId,
        CancellationToken ct = default);

    /// <summary>
    /// ویرایش متن پیام قبلی. اگه keyboard بدی، دکمه‌ها هم عوض می‌شن.
    /// </summary>
    Task<bool> EditMessageTextAsync(
        long chatId,
        long messageId,
        string text,
        BaleInlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default);
}