namespace PharmacyMiddleware.Services;

/// <summary>
/// منطق پیگیری وضعیت سفارش
/// </summary>
public interface IOrderTrackingService
{
    /// <summary>
    /// استعلام سفارش با کد پیگیری.
    /// خطاها رو به نتیجه‌ی معنادار تبدیل می‌کنه.
    /// هرگز Exception پرتاب نمی‌کنه.
    /// </summary>
    Task<OrderTrackingResult> TrackAsync(string? trackingCode, CancellationToken ct = default);
}