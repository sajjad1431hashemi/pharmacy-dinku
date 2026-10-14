namespace PharmacyMiddleware.State;

/// <summary>
/// مدیریت وضعیت کاربران توی ربات.
/// فعلاً In-Memory، بعداً می‌شه بردش روی Redis یا DB.
/// </summary>
public interface IUserStateStore
{
    /// <summary>
    /// گرفتن وضعیت کاربر. اگه وجود نداشت، یه وضعیت جدید می‌سازه و برمی‌گردونه.
    /// </summary>
    UserState GetOrCreate(long chatId);

    /// <summary>
    /// ذخیره‌ی وضعیت کاربر (آپدیت).
    /// </summary>
    void Save(UserState state);

    /// <summary>
    /// گرفتن وضعیت کاربر اگه وجود داره، در غیر این صورت null.
    /// </summary>
    UserState? TryGet(long chatId);
}