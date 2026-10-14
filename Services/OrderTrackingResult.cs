using PharmacyMiddleware.Contracts;

namespace PharmacyMiddleware.Services;

public enum OrderTrackingOutcome
{
    /// <summary>
    /// سفارش پیدا شد و اطلاعات موجوده
    /// </summary>
    Success = 0,

    /// <summary>
    /// کد فرمتش غلط بود (اصلاً به API نرفتیم)
    /// </summary>
    InvalidFormat = 1,

    /// <summary>
    /// کد فرمتش درست بود ولی سفارشی با این کد پیدا نشد
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// خطا در ارتباط با API (شبکه، سرور، Timeout و...)
    /// </summary>
    ApiError = 3
}

/// <summary>
/// نتیجه‌ی استعلام وضعیت سفارش
/// </summary>
public sealed class OrderTrackingResult
{
    public OrderTrackingOutcome Outcome { get; init; }

    public OrderDto? Order { get; init; }

    /// <summary>
    /// اگه خطا بود، توضیح اضافی برای لاگ
    /// </summary>
    public string? ErrorDetail { get; init; }

    // ────────────────────────────────────────
    // Factory Methods
    // ────────────────────────────────────────
    public static OrderTrackingResult Success(OrderDto order) => new()
    {
        Outcome = OrderTrackingOutcome.Success,
        Order = order
    };

    public static OrderTrackingResult InvalidFormat() => new()
    {
        Outcome = OrderTrackingOutcome.InvalidFormat
    };

    public static OrderTrackingResult NotFound() => new()
    {
        Outcome = OrderTrackingOutcome.NotFound
    };

    public static OrderTrackingResult ApiError(string? detail = null) => new()
    {
        Outcome = OrderTrackingOutcome.ApiError,
        ErrorDetail = detail
    };
}