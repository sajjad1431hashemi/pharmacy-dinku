using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PharmacyMiddleware.Configuration;
using PharmacyMiddleware.Handlers;
using PharmacyMiddleware.Models.Bale;

namespace PharmacyMiddleware.Controllers;

[ApiController]
[Route("api/bale")]
public sealed class BaleWebhookController : ControllerBase
{
    private readonly IBaleUpdateHandler _handler;
    private readonly BaleOptions _options;
    private readonly ILogger<BaleWebhookController> _logger;

    public BaleWebhookController(
        IBaleUpdateHandler handler,
        IOptions<BaleOptions> options,
        ILogger<BaleWebhookController> logger)
    {
        _handler = handler;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// دریافت Updateها از بله.
    /// مسیر: POST /api/bale/webhook/{secret}
    /// </summary>
    [HttpPost("webhook/{secret}")]
    public async Task<IActionResult> Webhook(
        [FromRoute] string secret,
        [FromBody] BaleUpdate update,
        CancellationToken ct)
    {
        // ── لاگ ۱: ورود به endpoint ──
        _logger.LogInformation(
            ">>> [1] Webhook called. Secret={Secret}, UpdateId={UpdateId}, " +
            "HasMessage={HasMessage}, HasCallback={HasCallback}, " +
            "MessageText={MessageText}",
            secret,
            update.UpdateId,
            update.Message is not null,
            update.CallbackQuery is not null,
            update.Message?.Text ?? update.CallbackQuery?.Data);

        // ── بررسی Secret ──
        if (!string.Equals(secret, _options.WebhookSecret, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "!!! [2] Secret mismatch. Expected length: {ExpectedLen}, Got length: {GotLen}",
                _options.WebhookSecret?.Length ?? 0,
                secret?.Length ?? 0);

            return NotFound();
        }

        _logger.LogInformation(">>> [3] Secret verified. Calling handler...");

        // ── پردازش ──
        try
        {
            await _handler.HandleAsync(update, ct);
            _logger.LogInformation(">>> [4] Handler completed successfully. UpdateId={UpdateId}",
                update.UpdateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "!!! [ERR] Handler threw exception for UpdateId={UpdateId}",
                update.UpdateId);
            throw;
        }

        _logger.LogInformation(">>> [5] Returning OK for UpdateId={UpdateId}", update.UpdateId);

        return Ok();
    }

    /// <summary>
    /// Endpoint برای تست سلامت
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", time = DateTime.UtcNow });
}