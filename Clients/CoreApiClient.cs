using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmacyMiddleware.Configuration;
using PharmacyMiddleware.Contracts;

namespace PharmacyMiddleware.Clients;

public sealed class CoreApiClient : ICoreApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CoreApiClient> _logger;

    public CoreApiClient(
        HttpClient http,
        IOptions<CoreApiOptions> options,
        ILogger<CoreApiClient> logger)
    {
        _http = http;
        _logger = logger;

        var opt = options.Value;
        _http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(opt.TimeoutSeconds);

        if (!string.IsNullOrWhiteSpace(opt.ApiKey))
            _http.DefaultRequestHeaders.Add("X-Api-Key", opt.ApiKey);
    }

    // ───────────────────────────────────────────
    // Track Order
    // ───────────────────────────────────────────
    public async Task<OrderDto?> TrackOrderAsync(string trackingCode, CancellationToken ct = default)
    {
        var url = $"api/Orders/track/{Uri.EscapeDataString(trackingCode)}";

        _logger.LogInformation("Calling Core API: {Url}", url);

        using var response = await _http.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.NoContent)
        {
            _logger.LogInformation("Order not found: {Code}", trackingCode);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Core API error: {Status} for {Code}",
                (int)response.StatusCode, trackingCode);

            throw new HttpRequestException(
                $"Core API returned {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken: ct);

        _logger.LogInformation("Order fetched: {Code}, Status={Status}",
            trackingCode, order?.Status);

        return order;
    }

    // ───────────────────────────────────────────
    // Submit Feedback
    // ───────────────────────────────────────────
    public async Task<bool> SubmitFeedbackAsync(
        string trackingCode,
        int rating,
        string? comment,
        CancellationToken ct = default)
    {
        var url = $"api/Orders/track/{Uri.EscapeDataString(trackingCode)}/feedback";

        var payload = new
        {
            rating,
            comment
        };

        _logger.LogInformation("Submitting feedback: {Code}, Rating={Rating}", trackingCode, rating);

        using var response = await _http.PostAsJsonAsync(url, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Feedback failed: {Status} — {Body}",
                (int)response.StatusCode, body);
            return false;
        }

        _logger.LogInformation("Feedback submitted successfully: {Code}", trackingCode);
        return true;
    }
}