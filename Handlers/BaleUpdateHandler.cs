using Microsoft.Extensions.Logging;
using PharmacyMiddleware.Clients;
using PharmacyMiddleware.Contracts;
using PharmacyMiddleware.Models.Bale;
using PharmacyMiddleware.Services;
using PharmacyMiddleware.State;
using PharmacyMiddleware.Utilities;

namespace PharmacyMiddleware.Handlers;

public sealed class BaleUpdateHandler : IBaleUpdateHandler
{
    private readonly IBaleBotClient _bale;
    private readonly ICoreApiClient _coreApi;
    private readonly IOrderTrackingService _tracking;
    private readonly IMessageFormatter _formatter;
    private readonly IUserStateStore _stateStore;
    private readonly ILogger<BaleUpdateHandler> _logger;

    public BaleUpdateHandler(
        IBaleBotClient bale,
        ICoreApiClient coreApi,
        IOrderTrackingService tracking,
        IMessageFormatter formatter,
        IUserStateStore stateStore,
        ILogger<BaleUpdateHandler> logger)
    {
        _bale = bale;
        _coreApi = coreApi;
        _tracking = tracking;
        _formatter = formatter;
        _stateStore = stateStore;
        _logger = logger;
    }

    // ════════════════════════════════════════════
    //  نقطه‌ی ورود
    // ════════════════════════════════════════════
    public async Task HandleAsync(BaleUpdate update, CancellationToken ct = default)
    {
        try
        {
            if (update.Message is not null)
            {
                await HandleMessageAsync(update.Message, ct);
                return;
            }

            if (update.CallbackQuery is not null)
            {
                await HandleCallbackAsync(update.CallbackQuery, ct);
                return;
            }

            _logger.LogDebug("Unhandled update type: {UpdateId}", update.UpdateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update {UpdateId}", update.UpdateId);
        }
    }

    // ════════════════════════════════════════════
    //  پیام متنی
    // ════════════════════════════════════════════
    private async Task HandleMessageAsync(BaleMessage message, CancellationToken ct)
    {
        var chatId = message.Chat?.Id ?? 0;
        var text = message.Text?.Trim();

        if (chatId == 0) return;

        _logger.LogInformation("Message received: ChatId={ChatId}, Text={Text}", chatId, text);

        var state = _stateStore.GetOrCreate(chatId);

        // ── ۱. اگه توی حالت AwaitingFeedback هستیم و پیام /start نیست → ثبت نظر ──
        if (state.ConversationState == UserConversationState.AwaitingFeedback
            && !string.Equals(text, "/start", StringComparison.OrdinalIgnoreCase))
        {
            await HandleFeedbackSubmissionAsync(chatId, state, text, ct);
            return;
        }

        // ── ۲. اگه وسط امتیاز/نظر بود و کار دیگه‌ای کرد → کنسل ──
        if (state.ConversationState != UserConversationState.Idle)
        {
            state.ConversationState = UserConversationState.Idle;
            state.PendingRating = null;
            _stateStore.Save(state);
            _logger.LogInformation("Conversation cancelled by user action: ChatId={ChatId}", chatId);
        }

        // ── ۳. دستور /start ──
        if (string.Equals(text, "/start", StringComparison.OrdinalIgnoreCase))
        {
            await _bale.SendMessageAsync(chatId, BotMessages.Welcome, keyboard: null, ct);
            return;
        }

        // ── ۴. کد پیگیری ──
        await HandleTrackingCodeAsync(chatId, state, text, ct);
    }

    // ════════════════════════════════════════════
    //  دکمه‌ی Inline
    // ════════════════════════════════════════════
    private async Task HandleCallbackAsync(BaleCallbackQuery query, CancellationToken ct)
    {
        var chatId = query.Message?.Chat?.Id ?? 0;
        var data = query.Data;

        if (chatId == 0) return;

        _logger.LogInformation("Callback received: ChatId={ChatId}, Data={Data}", chatId, data);

        var state = _stateStore.GetOrCreate(chatId);

        // ── دکمه‌های ستاره ──
        var rating = BotMessages.ParseRatingCallback(data);
        if (rating.HasValue)
        {
            await HandleRatingSelectedAsync(chatId, state, rating.Value, ct);
            return;
        }

        switch (data)
        {
            case BotMessages.CallbackRefresh:
                await HandleRefreshAsync(chatId, state, ct);
                break;

            case BotMessages.CallbackFeedback:
                await HandleFeedbackPromptAsync(chatId, state, ct);
                break;

            case BotMessages.CallbackBack:
                await HandleRefreshAsync(chatId, state, ct);
                break;

            case BotMessages.CallbackCancelFeedback:
                await HandleCancelFeedbackAsync(chatId, state, ct);
                break;

            case BotMessages.CallbackRetryFeedback:
                await HandleFeedbackPromptAsync(chatId, state, ct);
                break;

            default:
                _logger.LogWarning("Unknown callback data: {Data}", data);
                break;
        }
    }

    // ════════════════════════════════════════════
    //  منطق ۱: کد پیگیری
    // ════════════════════════════════════════════
    private async Task HandleTrackingCodeAsync(
        long chatId, UserState state, string? text, CancellationToken ct)
    {
        var result = await _tracking.TrackAsync(text, ct);

        switch (result.Outcome)
        {
            case OrderTrackingOutcome.InvalidFormat:
                await _bale.SendMessageAsync(chatId, BotMessages.InvalidTrackingCode, null, ct);
                return;

            case OrderTrackingOutcome.NotFound:
                await _bale.SendMessageAsync(chatId, BotMessages.OrderNotFound, null, ct);
                return;

            case OrderTrackingOutcome.ApiError:
                await _bale.SendMessageAsync(chatId, BotMessages.ApiError, null, ct);
                return;

            case OrderTrackingOutcome.Success:
                state.LastTrackingCode = TrackingCodeValidator.Normalize(text);
                _stateStore.Save(state);
                await SendOrderStatusAsync(chatId, state, result.Order!, ct);
                return;
        }
    }

    // ════════════════════════════════════════════
    //  منطق ۲: پیگیری مجدد
    // ════════════════════════════════════════════
    private async Task HandleRefreshAsync(long chatId, UserState state, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(state.LastTrackingCode))
        {
            await _bale.SendMessageAsync(chatId, BotMessages.InvalidTrackingCode, null, ct);
            return;
        }

        // حذف پیام وضعیت قبلی
        if (state.CurrentStatusMessageId.HasValue)
        {
            await _bale.DeleteMessageAsync(chatId, state.CurrentStatusMessageId.Value, ct);
            state.CurrentStatusMessageId = null;
            _stateStore.Save(state);
        }

        var result = await _tracking.TrackAsync(state.LastTrackingCode, ct);

        switch (result.Outcome)
        {
            case OrderTrackingOutcome.InvalidFormat:
                await _bale.SendMessageAsync(chatId, BotMessages.InvalidTrackingCode, null, ct);
                return;

            case OrderTrackingOutcome.NotFound:
                await _bale.SendMessageAsync(chatId, BotMessages.OrderNotFound, null, ct);
                return;

            case OrderTrackingOutcome.ApiError:
                await _bale.SendMessageAsync(chatId, BotMessages.ApiError, null, ct);
                return;

            case OrderTrackingOutcome.Success:
                await SendOrderStatusAsync(chatId, state, result.Order!, ct);
                return;
        }
    }

    // ════════════════════════════════════════════
    //  منطق ۳: پرامپت امتیاز
    // ════════════════════════════════════════════
    private async Task HandleFeedbackPromptAsync(long chatId, UserState state, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(state.LastTrackingCode))
        {
            await _bale.SendMessageAsync(chatId, BotMessages.InvalidTrackingCode, null, ct);
            return;
        }

        state.ConversationState = UserConversationState.AwaitingRating;
        state.PendingRating = null;
        _stateStore.Save(state);

        var keyboard = _formatter.BuildRatingKeyboard();
        var messageId = await _bale.SendMessageAsync(
            chatId, BotMessages.RatingPrompt, keyboard, ct);

        state.RatingPromptMessageId = messageId;
        _stateStore.Save(state);
    }

    // ════════════════════════════════════════════
    //  منطق ۴: انتخاب امتیاز
    // ════════════════════════════════════════════
    private async Task HandleRatingSelectedAsync(
        long chatId, UserState state, int rating, CancellationToken ct)
    {
        if (state.ConversationState != UserConversationState.AwaitingRating)
        {
            _logger.LogWarning("Rating received but state is {State}", state.ConversationState);
            return;
        }

        state.PendingRating = rating;
        state.ConversationState = UserConversationState.AwaitingFeedback;
        _stateStore.Save(state);

        var stars = BotMessages.StarsFor(rating);
        var text = string.Format(BotMessages.FeedbackPromptFormat, stars, rating);

        // اگه message_id رو داریم، Edit کن
        if (state.RatingPromptMessageId.HasValue)
        {
            await _bale.EditMessageTextAsync(
                chatId,
                state.RatingPromptMessageId.Value,
                text,
                _formatter.BuildCancelOnlyKeyboard(),
                ct);
        }
        else
        {
            var msgId = await _bale.SendMessageAsync(
                chatId, text, _formatter.BuildCancelOnlyKeyboard(), ct);
            state.RatingPromptMessageId = msgId;
            _stateStore.Save(state);
        }
    }

    // ════════════════════════════════════════════
    //  منطق ۵: ثبت نظر
    // ════════════════════════════════════════════
    private async Task HandleFeedbackSubmissionAsync(
        long chatId, UserState state, string? feedback, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(feedback))
        {
            await _bale.SendMessageAsync(chatId, BotMessages.FeedbackEmptyComment, null, ct);
            return;
        }

        if (!state.PendingRating.HasValue || string.IsNullOrWhiteSpace(state.LastTrackingCode))
        {
            _logger.LogWarning("Feedback without rating or code: ChatId={ChatId}", chatId);
            state.ConversationState = UserConversationState.Idle;
            state.PendingRating = null;
            _stateStore.Save(state);
            return;
        }

        var rating = state.PendingRating.Value;
        var code = state.LastTrackingCode;

        // ارسال به API
        var success = await _coreApi.SubmitFeedbackAsync(code, rating, feedback, ct);

        if (!success)
        {
            state.ConversationState = UserConversationState.AwaitingFeedback;
            _stateStore.Save(state);

            await _bale.SendMessageAsync(
                chatId, BotMessages.FeedbackError, _formatter.BuildErrorKeyboard(), ct);
            return;
        }

        state.ConversationState = UserConversationState.Idle;
        state.PendingRating = null;
        state.RatingPromptMessageId = null;
        _stateStore.Save(state);

        await _bale.SendMessageAsync(
            chatId, BotMessages.FeedbackThanks, _formatter.BuildBackKeyboard(), ct);
    }

    // ════════════════════════════════════════════
    //  منطق ۶: کنسل کردن روند امتیاز/نظر
    // ════════════════════════════════════════════
    private async Task HandleCancelFeedbackAsync(long chatId, UserState state, CancellationToken ct)
    {
        // حذف پیام پرامپت (ستاره‌ها یا "نظر بنویسید")
        if (state.RatingPromptMessageId.HasValue)
        {
            await _bale.DeleteMessageAsync(chatId, state.RatingPromptMessageId.Value, ct);
            state.RatingPromptMessageId = null;
        }

        state.ConversationState = UserConversationState.Idle;
        state.PendingRating = null;
        _stateStore.Save(state);

        _logger.LogInformation("Feedback conversation cancelled: ChatId={ChatId}", chatId);
    }

    // ════════════════════════════════════════════
    //  Helper: ارسال پیام وضعیت
    // ════════════════════════════════════════════
    private async Task SendOrderStatusAsync(
        long chatId, UserState state, OrderDto order, CancellationToken ct)
    {
        // اگه کاربر قبلاً نظر داده، دکمه‌ی ثبت پیشنهادات رو نذار
        var includeFeedback = !order.FeedbackRating.HasValue;

        var (text, keyboard) = _formatter.FormatOrderStatus(order, includeFeedback);

        var messageId = await _bale.SendMessageAsync(chatId, text, keyboard, ct);

        state.CurrentStatusMessageId = messageId;
        _stateStore.Save(state);
    }
}