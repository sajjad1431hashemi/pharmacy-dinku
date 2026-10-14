using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PharmacyMiddleware.Configuration;
using PharmacyMiddleware.Models.Bale;

namespace PharmacyMiddleware.Clients;

public sealed class BaleBotClient : IBaleBotClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly ILogger<BaleBotClient> _logger;
    private readonly string _botToken;

    public BaleBotClient(
        HttpClient http,
        IOptions<BaleOptions> options,
        ILogger<BaleBotClient> logger)
    {
        _http = http;
        _logger = logger;

        var opt = options.Value;
        _http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(15);

        _botToken = opt.BotToken;
    }

    // ───────────────────────────────────────────
    // Send Message
    // ───────────────────────────────────────────
    public async Task<long> SendMessageAsync(
        long chatId,
        string text,
        BaleInlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default)
    {
        var payload = new SendMessagePayload
        {
            ChatId = chatId,
            Text = text,
            ReplyMarkup = keyboard
        };

        var url = $"./bot{_botToken}/sendMessage";

        using var response = await _http.PostAsJsonAsync(url, payload, JsonOpts, ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Bale sendMessage failed: {Status} — {Body}",
                (int)response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        _logger.LogDebug("Bale sendMessage raw response: {Body}", body);

        var result = JsonSerializer.Deserialize<SendMessageResult>(body, JsonOpts);
        var messageId = result?.Result?.MessageId ?? 0;

        _logger.LogInformation("Bale message sent: ChatId={ChatId}, MessageId={MessageId}",
            chatId, messageId);

        return messageId;
    }

    // ───────────────────────────────────────────
    // Delete Message
    // ───────────────────────────────────────────
    public async Task<bool> DeleteMessageAsync(
        long chatId,
        long messageId,
        CancellationToken ct = default)
    {
        var payload = new DeleteMessagePayload
        {
            ChatId = chatId,
            MessageId = messageId
        };

        var url = $"./bot{_botToken}/deleteMessage";

        using var response = await _http.PostAsJsonAsync(url, payload, JsonOpts, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Bale deleteMessage failed: {Status} — {Body}",
                (int)response.StatusCode, body);
            return false;
        }

        _logger.LogInformation("Bale message deleted: ChatId={ChatId}, MessageId={MessageId}",
            chatId, messageId);

        return true;
    }

    // ───────────────────────────────────────────
    // Edit Message Text
    // ───────────────────────────────────────────
    public async Task<bool> EditMessageTextAsync(
        long chatId,
        long messageId,
        string text,
        BaleInlineKeyboardMarkup? keyboard = null,
        CancellationToken ct = default)
    {
        var payload = new EditMessageTextPayload
        {
            ChatId = chatId,
            MessageId = messageId,
            Text = text,
            ReplyMarkup = keyboard
        };

        var url = $"./bot{_botToken}/editMessageText";

        using var response = await _http.PostAsJsonAsync(url, payload, JsonOpts, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Bale editMessageText failed: {Status} — {Body}",
                (int)response.StatusCode, body);
            return false;
        }

        _logger.LogInformation("Bale message edited: ChatId={ChatId}, MessageId={MessageId}",
            chatId, messageId);

        return true;
    }

    // ═══════════════════════════════════════════
    //  Payload models (strongly-typed)
    // ═══════════════════════════════════════════
    private sealed class SendMessagePayload
    {
        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("reply_markup")]
        public BaleInlineKeyboardMarkup? ReplyMarkup { get; set; }
    }

    private sealed class DeleteMessagePayload
    {
        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }

        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }
    }

    private sealed class EditMessageTextPayload
    {
        [JsonPropertyName("chat_id")]
        public long ChatId { get; set; }

        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("reply_markup")]
        public BaleInlineKeyboardMarkup? ReplyMarkup { get; set; }
    }

    // ═══════════════════════════════════════════
    //  Response models
    // ═══════════════════════════════════════════
    private sealed class SendMessageResult
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("result")]
        public MessageResult? Result { get; set; }
    }

    private sealed class MessageResult
    {
        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }
    }
}