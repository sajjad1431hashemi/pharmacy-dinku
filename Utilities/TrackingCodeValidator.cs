using System.Text;
using System.Text.RegularExpressions;

namespace PharmacyMiddleware.Utilities;

public static partial class TrackingCodeValidator
{
    // کد پیگیری: ۶ تا ۹ رقم
    [GeneratedRegex(@"^\d{6,9}$", RegexOptions.Compiled)]
    private static partial Regex TrackingCodeRegex();

    /// <summary>
    /// نرمال‌سازی کد پیگیری:
    /// - اعداد فارسی (۰-۹) به انگلیسی
    /// - اعداد عربی (٠-٩) به انگلیسی
    /// - حذف فاصله و کاراکترهای غیر عددی
    /// </summary>
    public static string Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        var sb = new StringBuilder(code.Length);

        foreach (var ch in code)
        {
            var normalized = ch switch
            {
                // اعداد فارسی ۰-۹ (U+06F0 to U+06F9)
                >= '\u06F0' and <= '\u06F9' => (char)(ch - '\u06F0' + '0'),

                // اعداد عربی ٠-٩ (U+0660 to U+0669)
                >= '\u0660' and <= '\u0669' => (char)(ch - '\u0660' + '0'),

                // اعداد انگلیسی
                >= '0' and <= '9' => ch,

                _ => '\0' // بقیه رو نادیده بگیر
            };

            if (normalized != '\0')
                sb.Append(normalized);
        }

        return sb.ToString();
    }

    /// <summary>
    /// چک می‌کنه که کد پیگیری معتبره یا نه.
    /// اول نرمال‌سازی می‌کنه، بعد Regex می‌زنه.
    /// </summary>
    public static bool IsValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var normalized = Normalize(code);

        return TrackingCodeRegex().IsMatch(normalized);
    }
}