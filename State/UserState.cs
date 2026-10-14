namespace PharmacyMiddleware.State;

/// <summary>
/// حالت‌های ممکن برای هر کاربر توی ربات
/// </summary>
public enum UserConversationState
{
    /// <summary>
    /// حالت عادی — منتظر کد پیگیری هستیم
    /// </summary>
    Idle = 0,

    /// <summary>
    /// منتظر انتخاب امتیاز از دکمه‌های ستاره
    /// </summary>
    AwaitingRating = 1,

    /// <summary>
    /// امتیاز گرفته شد، منتظر متن نظر هستیم
    /// </summary>
    AwaitingFeedback = 2
}

/// <summary>
/// اطلاعات session هر کاربر توی ربات
/// </summary>
public sealed class UserState
{
    public long ChatId { get; set; }

    /// <summary>
    /// آخرین کد پیگیری‌ای که کاربر فرستاده
    /// </summary>
    public string? LastTrackingCode { get; set; }

    /// <summary>
    /// MessageId آخرین پیام وضعیت سفارش (برای حذف کردنش)
    /// </summary>
    public long? CurrentStatusMessageId { get; set; }

    /// <summary>
    /// MessageId پیام پرامپت امتیاز (برای Edit یا حذف)
    /// </summary>
    public long? RatingPromptMessageId { get; set; }

    /// <summary>
    /// امتیازی که کاربر انتخاب کرده (۱ تا ۵) — منتظر متن نظر
    /// </summary>
    public int? PendingRating { get; set; }

    /// <summary>
    /// حالت فعلی گفتگو
    /// </summary>
    public UserConversationState ConversationState { get; set; } = UserConversationState.Idle;

    /// <summary>
    /// آخرین زمان فعالیت (برای پاکسازی بعدی)
    /// </summary>
    public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;
}