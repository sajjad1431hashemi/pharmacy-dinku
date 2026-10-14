namespace PharmacyMiddleware.Utilities;

public static class BotMessages
{
    // ─────────────────────────────────────
    // خوش‌آمدگویی
    // ─────────────────────────────────────
    public const string Welcome =
        "سلام 👋\n\n" +
        "🏥 ربات دستیار اطلاع‌رسانی داروخانه دکتر دینکو\n\n" +
        "برای مشاهده وضعیت سفارش، کد پیگیری دریافت‌شده از برنامه را ارسال کنید.";

    // ─────────────────────────────────────
    // خطاها
    // ─────────────────────────────────────
    public const string InvalidTrackingCode =
        "❌ کد پیگیری ارسال شده نامعتبر می‌باشد.\n\n" +
        "لطفاً کد پیگیری دریافت‌شده از برنامه را ارسال کنید.";

    public const string OrderNotFound =
        "❌ سفارشی با این کد پیگیری یافت نشد.\n\n" +
        "لطفاً کد را بررسی و دوباره ارسال کنید.";

    public const string ApiError =
        "⚠️ در حال حاضر امکان دریافت اطلاعات نیست.\n\n" +
        "لطفاً چند لحظه بعد دوباره تلاش کنید.";

    // ─────────────────────────────────────
    // امتیاز و نظر
    // ─────────────────────────────────────
    public const string RatingPrompt =
        "📝 لطفاً امتیاز خود را انتخاب کنید:";

    /// <summary>
    /// الگوی پیام بعد از انتخاب امتیاز. {0} = ستاره‌ها، {1} = عدد امتیاز
    /// </summary>
    public const string FeedbackPromptFormat =
        "📝 امتیاز شما: {0} ({1} از ۵)\n\n" +
        "حالا لطفاً نظر خود را بنویسید.";

    public const string FeedbackThanks =
        "✅ از پیام شما متشکریم.";

    public const string FeedbackEmptyComment =
        "⚠️ لطفاً متن نظر خود را بنویسید.";

    public const string FeedbackError =
        "⚠️ خطا در ثبت نظر. لطفاً دوباره تلاش کنید.";

    // ─────────────────────────────────────
    // دکمه‌ها
    // ─────────────────────────────────────
    public const string ButtonRefreshOrder = "🔄 پیگیری مجدد سفارش";
    public const string ButtonSubmitFeedback = "📝 ثبت پیشنهادات و انتقادات";
    public const string ButtonBackToTracking = "🔙 بازگشت به پیگیری سفارش";
    public const string ButtonCancel = "🔙 بازگشت";
    public const string ButtonRetry = "🔁 تلاش مجدد";

    // ─────────────────────────────────────
    // Callback Data
    // ─────────────────────────────────────
    public const string CallbackRefresh = "refresh_order";
    public const string CallbackFeedback = "submit_feedback";
    public const string CallbackBack = "back_to_tracking";
    public const string CallbackCancelFeedback = "cancel_feedback";
    public const string CallbackRetryFeedback = "retry_feedback";

    /// <summary>
    /// پیشوند Callback برای دکمه‌های ستاره. فرمت: feedback_rating:4
    /// </summary>
    public const string CallbackRatingPrefix = "feedback_rating:";

    // ─────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────

    /// <summary>
    /// ساخت رشته‌ی ستاره بر اساس امتیاز. مثال: 4 → ⭐⭐⭐⭐
    /// </summary>
    public static string StarsFor(int rating)
    {
        var r = Math.Clamp(rating, 0, 5);
        return string.Concat(Enumerable.Repeat("⭐", r));
    }

    /// <summary>
    /// ساخت Callback Data برای یه امتیاز خاص
    /// </summary>
    public static string RatingCallback(int rating) => $"{CallbackRatingPrefix}{rating}";

    /// <summary>
    /// استخراج امتیاز از Callback Data. اگه معتبر نبود، null.
    /// </summary>
    public static int? ParseRatingCallback(string? data)
    {
        if (string.IsNullOrWhiteSpace(data)) return null;
        if (!data.StartsWith(CallbackRatingPrefix, StringComparison.Ordinal)) return null;

        var value = data[CallbackRatingPrefix.Length..];
        if (int.TryParse(value, out var rating) && rating is >= 1 and <= 5)
            return rating;

        return null;
    }
}