namespace PharmacyMiddleware.Configuration;

public sealed class CoreApiOptions
{
    public const string SectionName = "CoreApi";

    /// <summary>
    /// آدرس پایه API اصلی، مثل http://87.107.164.132
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// اگه API نیاز به کلید داشت، اینجا ست می‌شه.
    /// فعلاً طبق swagger آزاده، ولی برای آینده نگه می‌داریم.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// تایم‌اوت هر درخواست به ثانیه
    /// </summary>
    public int TimeoutSeconds { get; set; } = 15;
}