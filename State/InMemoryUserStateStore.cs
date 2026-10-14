using System.Collections.Concurrent;

namespace PharmacyMiddleware.State;

/// <summary>
/// پیاده‌سازی In-Memory برای IUserStateStore.
/// thread-safe با ConcurrentDictionary.
/// </summary>
public sealed class InMemoryUserStateStore : IUserStateStore
{
    private readonly ConcurrentDictionary<long, UserState> _states = new();

    public UserState GetOrCreate(long chatId)
    {
        return _states.GetOrAdd(chatId, id => new UserState
        {
            ChatId = id,
            LastActivityUtc = DateTime.UtcNow
        });
    }

    public UserState? TryGet(long chatId)
    {
        _states.TryGetValue(chatId, out var state);
        return state;
    }

    public void Save(UserState state)
    {
        state.LastActivityUtc = DateTime.UtcNow;
        _states[state.ChatId] = state;
    }
}