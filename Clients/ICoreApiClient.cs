using PharmacyMiddleware.Contracts;

namespace PharmacyMiddleware.Clients;

public interface ICoreApiClient
{
    Task<OrderDto?> TrackOrderAsync(string trackingCode, CancellationToken ct = default);

    /// <summary>
    /// ثبت نظر و امتیاز برای یه سفارش.
    /// true اگه موفق، false اگه خطای 4xx/5xx.
    /// </summary>
    Task<bool> SubmitFeedbackAsync(
        string trackingCode,
        int rating,
        string? comment,
        CancellationToken ct = default);
}