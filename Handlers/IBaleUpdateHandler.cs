using PharmacyMiddleware.Models.Bale;

namespace PharmacyMiddleware.Handlers;

/// <summary>
/// پردازش‌کننده‌ی پیام‌ها و دکمه‌های بله
/// </summary>
public interface IBaleUpdateHandler
{
    /// <summary>
    /// پردازش یک Update دریافتی از بله
    /// </summary>
    Task HandleAsync(BaleUpdate update, CancellationToken ct = default);
}