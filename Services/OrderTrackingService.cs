using Microsoft.Extensions.Logging;
using PharmacyMiddleware.Clients;
using PharmacyMiddleware.Utilities;

namespace PharmacyMiddleware.Services;

public sealed class OrderTrackingService : IOrderTrackingService
{
    private readonly ICoreApiClient _coreApi;
    private readonly ILogger<OrderTrackingService> _logger;

    public OrderTrackingService(
        ICoreApiClient coreApi,
        ILogger<OrderTrackingService> logger)
    {
        _coreApi = coreApi;
        _logger = logger;
    }

    public async Task<OrderTrackingResult> TrackAsync(
        string? trackingCode,
        CancellationToken ct = default)
    {
        // ── مرحله ۱: اعتبارسنجی فرمت ──
        if (!TrackingCodeValidator.IsValid(trackingCode))
        {
            _logger.LogInformation("Invalid tracking code format: {Code}", trackingCode);
            return OrderTrackingResult.InvalidFormat();
        }

        var code = trackingCode!.Trim();

        // ── مرحله ۲: استعلام از API ──
        try
        {
            var order = await _coreApi.TrackOrderAsync(code, ct);

            if (order is null)
            {
                _logger.LogInformation("Order not found: {Code}", code);
                return OrderTrackingResult.NotFound();
            }

            _logger.LogInformation("Order tracked successfully: {Code}", code);
            return OrderTrackingResult.Success(order);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // Timeout — TaskCanceledException ولی کاربر کنسل نکرده
            _logger.LogWarning(ex, "Timeout while tracking order: {Code}", code);
            return OrderTrackingResult.ApiError("Request timeout");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error while tracking order: {Code}", code);
            return OrderTrackingResult.ApiError(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while tracking order: {Code}", code);
            return OrderTrackingResult.ApiError(ex.Message);
        }
    }
}