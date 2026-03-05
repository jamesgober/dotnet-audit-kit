using System.Collections.Concurrent;
using JG.AuditKit;
using JG.AuditKit.Abstractions;

namespace JG.AuditKit.Tests;

internal sealed class CollectingSink : IAuditSink
{
    private readonly ConcurrentQueue<AuditEntry> _entries = new();

    public IReadOnlyList<AuditEntry> Entries => _entries.ToArray();

    public ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _entries.Enqueue(entry);
        return ValueTask.CompletedTask;
    }
}

internal sealed class ThrowingSink : IAuditSink
{
    public ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("sink failure");
    }
}

internal sealed class SlowSink : IAuditSink
{
    private readonly TimeSpan _delay;

    public SlowSink()
        : this(TimeSpan.FromMilliseconds(500))
    {
    }

    public SlowSink(TimeSpan delay)
    {
        _delay = delay;
    }

    public async ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
    }
}

internal sealed class AlwaysAllowFilter : IAuditFilter
{
    public bool ShouldWrite(AuditEntry entry) => true;
}

internal sealed class AlwaysDenyFilter : IAuditFilter
{
    public bool ShouldWrite(AuditEntry entry) => false;
}

internal sealed class ActionPrefixFilter : IAuditFilter
{
    public bool ShouldWrite(AuditEntry entry)
    {
        return entry.Action.StartsWith("write", StringComparison.Ordinal);
    }
}

internal static class TestWait
{
    public static async Task UntilAsync(Func<bool> predicate, TimeSpan timeout)
    {
        DateTime deadline = DateTime.UtcNow.Add(timeout);
        while (!predicate())
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("Timed out waiting for condition.");
            }

            await Task.Delay(20).ConfigureAwait(false);
        }
    }
}

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
