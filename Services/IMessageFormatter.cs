using PharmacyMiddleware.Contracts;
using PharmacyMiddleware.Models.Bale;

namespace PharmacyMiddleware.Services;

public interface IMessageFormatter
{
    /// <summary>
    /// ساخت پیام وضعیت سفارش.
    /// includeFeedbackButton: اگه false باشه، دکمه‌ی «ثبت پیشنهادات» نمایش داده نمی‌شه.
    /// </summary>
    (string Text, BaleInlineKeyboardMarkup Keyboard) FormatOrderStatus(
        OrderDto order,
        bool includeFeedbackButton);

    /// <summary>
    /// کیبورد اصلی (پیگیری مجدد + اختیاری ثبت پیشنهادات)
    /// </summary>
    BaleInlineKeyboardMarkup BuildMainKeyboard(bool includeFeedbackButton);

    /// <summary>
    /// کیبورد ستاره‌های امتیاز + دکمه‌ی بازگشت
    /// </summary>
    BaleInlineKeyboardMarkup BuildRatingKeyboard();

    /// <summary>
    /// کیبورد فقط با دکمه‌ی بازگشت (توی مرحله‌ی نوشتن نظر)
    /// </summary>
    BaleInlineKeyboardMarkup BuildCancelOnlyKeyboard();

    /// <summary>
    /// کیبورد پیام تشکر (بازگشت به پیگیری)
    /// </summary>
    BaleInlineKeyboardMarkup BuildBackKeyboard();

    /// <summary>
    /// کیبورد خطا (تلاش مجدد + بازگشت)
    /// </summary>
    BaleInlineKeyboardMarkup BuildErrorKeyboard();
}