using Microsoft.AspNetCore.Http;

namespace EPR.Calculator.Frontend.UnitTests.Mocks;

/// <summary>
///     An <see cref="ISession" /> that keeps entries as the raw bytes it was given.
/// </summary>
/// <remarks>
///     Unlike <see cref="MockHttpSession" />, values survive a round trip intact, so the binary
///     encoding used by <c>SetInt32</c>/<c>GetInt32</c> works as it would in a real session.
/// </remarks>
public sealed class InMemorySession : ISession
{
    private readonly Dictionary<string, byte[]> store = new();

    public IEnumerable<string> Keys => store.Keys;

    public string Id { get; } = Guid.NewGuid().ToString();

    public bool IsAvailable => true;

    public void Clear() => store.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => store.Remove(key);

    public void Set(string key, byte[] value) => store[key] = value;

    public bool TryGetValue(string key, out byte[] value) => store.TryGetValue(key, out value!);
}
