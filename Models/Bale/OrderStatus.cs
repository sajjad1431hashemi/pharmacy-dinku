namespace PharmacyMiddleware.Models;

public enum OrderStatus
{
    Unknown = 0,
    Pending,
    UnderReview,
    Preparing,
    Ready,
    Delivered
}

public static class OrderStatusExtensions
{
    /// <summary>
    /// تبدیل رشته‌ی API به enum. اگه ناشناخته بود، Unknown برمی‌گردونه.
    /// </summary>
    public static OrderStatus ParseStatus(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return OrderStatus.Unknown;

        return raw.Trim().ToUpperInvariant() switch
        {
            "PENDING" => OrderStatus.Pending,
            "UNDERREVIEW" => OrderStatus.UnderReview,
            "PREPARING" => OrderStatus.Preparing,
            "READY" => OrderStatus.Ready,
            "DELIVERED" => OrderStatus.Delivered,
            _ => OrderStatus.Unknown
        };
    }

    /// <summary>
    /// متن فارسی هر وضعیت.
    /// </summary>
    public static string ToPersian(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "ثبت شده",
        OrderStatus.UnderReview => "در حال بررسی",
        OrderStatus.Preparing => "در حال آماده‌سازی",
        OrderStatus.Ready => "آماده تحویل",
        OrderStatus.Delivered => "تحویل داده شده",
        _ => "نامشخص"
    };

    /// <summary>
    /// ترتیب منطقی مراحل (برای مقایسه).
    /// </summary>
    public static int ToOrder(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => 1,
        OrderStatus.UnderReview => 2,
        OrderStatus.Preparing => 3,
        OrderStatus.Ready => 4,
        OrderStatus.Delivered => 5,
        _ => 0
    };

    /// <summary>
    /// لیست مرتب همه‌ی مراحل اصلی (برای نمایش پیشرفت).
    /// </summary>
    public static readonly OrderStatus[] AllSteps =
    {
        OrderStatus.Pending,
        OrderStatus.UnderReview,
        OrderStatus.Preparing,
        OrderStatus.Ready,
        OrderStatus.Delivered
    };
}