using System.Text;
using PharmacyMiddleware.Contracts;
using PharmacyMiddleware.Models;
using PharmacyMiddleware.Models.Bale;
using PharmacyMiddleware.Utilities;

namespace PharmacyMiddleware.Services;

public sealed class MessageFormatter : IMessageFormatter
{
    // ────────────────────────────────────────────
    // پیام وضعیت سفارش
    // ────────────────────────────────────────────
    public (string Text, BaleInlineKeyboardMarkup Keyboard) FormatOrderStatus(
        OrderDto order,
        bool includeFeedbackButton)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(order.PatientName))
            sb.AppendLine($"بیمار: {order.PatientName}");

        if (!string.IsNullOrWhiteSpace(order.TrackingCode))
            sb.AppendLine($"🔖 کد پیگیری: {order.TrackingCode}");

        sb.AppendLine($"📅 تاریخ ثبت: {PersianDate.ToPersianString(order.CreatedAt)}");
        sb.AppendLine();

        var currentStatus = GetCurrentStatus(order);

        sb.AppendLine($"🔄 وضعیت فعلی سفارش: {currentStatus.ToPersian()}");
        sb.AppendLine();

        if (order.Items is { Count: > 0 })
        {
            sb.AppendLine("💊 داروهای سفارش:");
            foreach (var item in order.Items)
                sb.AppendLine(FormatItem(item));
            sb.AppendLine();
        }

        sb.AppendLine("پیشرفت سفارش:");
        foreach (var step in OrderStatusExtensions.AllSteps)
            sb.AppendLine(FormatProgressStep(step, currentStatus));

        var text = sb.ToString().TrimEnd();

        return (text, BuildMainKeyboard(includeFeedbackButton));
    }

    // ────────────────────────────────────────────
    // کیبوردها
    // ────────────────────────────────────────────
    public BaleInlineKeyboardMarkup BuildMainKeyboard(bool includeFeedbackButton)
    {
        var rows = new List<List<BaleInlineKeyboardButton>>
        {
            new()
            {
                new BaleInlineKeyboardButton
                {
                    Text = BotMessages.ButtonRefreshOrder,
                    CallbackData = BotMessages.CallbackRefresh
                }
            }
        };

        if (includeFeedbackButton)
        {
            rows.Add(new List<BaleInlineKeyboardButton>
            {
                new BaleInlineKeyboardButton
                {
                    Text = BotMessages.ButtonSubmitFeedback,
                    CallbackData = BotMessages.CallbackFeedback
                }
            });
        }

        return new BaleInlineKeyboardMarkup { InlineKeyboard = rows };
    }

    public BaleInlineKeyboardMarkup BuildRatingKeyboard()
    {
        var starsRow = new List<BaleInlineKeyboardButton>();
        for (var i = 1; i <= 5; i++)
        {
            starsRow.Add(new BaleInlineKeyboardButton
            {
                Text = $"⭐{i}",
                CallbackData = BotMessages.RatingCallback(i)
            });
        }

        return new BaleInlineKeyboardMarkup
        {
            InlineKeyboard = new List<List<BaleInlineKeyboardButton>>
            {
                starsRow,
                new()
                {
                    new BaleInlineKeyboardButton
                    {
                        Text = BotMessages.ButtonCancel,
                        CallbackData = BotMessages.CallbackCancelFeedback
                    }
                }
            }
        };
    }

    public BaleInlineKeyboardMarkup BuildCancelOnlyKeyboard()
    {
        return new BaleInlineKeyboardMarkup
        {
            InlineKeyboard = new List<List<BaleInlineKeyboardButton>>
            {
                new()
                {
                    new BaleInlineKeyboardButton
                    {
                        Text = BotMessages.ButtonCancel,
                        CallbackData = BotMessages.CallbackCancelFeedback
                    }
                }
            }
        };
    }

    public BaleInlineKeyboardMarkup BuildBackKeyboard()
    {
        return new BaleInlineKeyboardMarkup
        {
            InlineKeyboard = new List<List<BaleInlineKeyboardButton>>
            {
                new()
                {
                    new BaleInlineKeyboardButton
                    {
                        Text = BotMessages.ButtonBackToTracking,
                        CallbackData = BotMessages.CallbackBack
                    }
                }
            }
        };
    }

    public BaleInlineKeyboardMarkup BuildErrorKeyboard()
    {
        return new BaleInlineKeyboardMarkup
        {
            InlineKeyboard = new List<List<BaleInlineKeyboardButton>>
            {
                new()
                {
                    new BaleInlineKeyboardButton
                    {
                        Text = BotMessages.ButtonRetry,
                        CallbackData = BotMessages.CallbackRetryFeedback
                    }
                },
                new()
                {
                    new BaleInlineKeyboardButton
                    {
                        Text = BotMessages.ButtonCancel,
                        CallbackData = BotMessages.CallbackCancelFeedback
                    }
                }
            }
        };
    }

    // ────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────
    private static OrderStatus GetCurrentStatus(OrderDto order)
    {
        if (order.StatusHistory is { Count: > 0 })
        {
            var highest = order.StatusHistory
                .Select(h => OrderStatusExtensions.ParseStatus(h.Status))
                .Where(s => s != OrderStatus.Unknown)
                .DefaultIfEmpty(OrderStatus.Unknown)
                .MaxBy(s => s.ToOrder());

            if (highest != OrderStatus.Unknown)
                return highest;
        }

        return OrderStatusExtensions.ParseStatus(order.Status);
    }

    private static string FormatItem(OrderItemDto item)
    {
        var name = string.IsNullOrWhiteSpace(item.Name) ? "نامشخص" : item.Name;
        var result = $"• {name} × {item.Quantity}";

        if (!string.IsNullOrWhiteSpace(item.Dosage))
            result += $" ({item.Dosage})";

        return result;
    }

    private static string FormatProgressStep(OrderStatus step, OrderStatus current)
    {
        var icon = step.ToOrder() switch
        {
            var ord when ord < current.ToOrder() => "✅",
            var ord when ord == current.ToOrder() => "🔄",
            _ => "⏳"
        };

        return $"{icon} {step.ToPersian()}";
    }
}