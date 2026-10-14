namespace PharmacyMiddleware.Configuration;

public sealed class BaleOptions
{
    public const string SectionName = "Bale";

    /// <summary>
    /// توکن ربات بله — از BotFather بله بگیر
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// آدرس پایه API بله
    /// </summary>
    public string BaseUrl { get; set; } = "https://tapi.bale.ai";

    /// <summary>
    /// این مسیر مخفی که توی Webhook URL قرار می‌گیره.
    /// فقط کسی که این رو بدونه می‌تونه به webhook ما POST کنه.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
}