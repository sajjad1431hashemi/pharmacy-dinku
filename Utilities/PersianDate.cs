using System.Globalization;

namespace PharmacyMiddleware.Utilities;

public static class PersianDate
{
    private static readonly PersianCalendar Calendar = new();

    /// <summary>
    /// تبدیل DateTime میلادی به رشته‌ی شمسی با فرمت: ۱۴۰۵/۰۷/۱۳ - ۱۳:۳۱
    /// </summary>
    public static string ToPersianString(DateTime dateTime)
    {
        // تبدیل به وقت ایران
        var tehran = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        var local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), tehran);

        var year = Calendar.GetYear(local);
        var month = Calendar.GetMonth(local);
        var day = Calendar.GetDayOfMonth(local);
        var hour = local.Hour;
        var minute = local.Minute;

        return $"{year:0000}/{month:00}/{day:00} - {hour:00}:{minute:00}";
    }
}