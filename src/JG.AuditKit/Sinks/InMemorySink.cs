using System.Collections.Concurrent;
using JG.AuditKit.Abstractions;

namespace JG.AuditKit.Sinks;

/// <summary>
/// Stores audit entries in memory for testing and diagnostics.
/// </summary>
/// <remarks>
/// This sink is thread-safe.
/// </remarks>
public sealed class InMemorySink : IAuditSink
{
    private readonly ConcurrentQueue<AuditEntry> _entries = new();

    /// <summary>
    /// Gets a snapshot of entries written to the sink.
    /// </summary>
    public IReadOnlyList<AuditEntry> Entries => _entries.ToArray();

    /// <summary>
    /// Gets the number of entries currently stored.
    /// </summary>
    public int Count => _entries.Count;

    /// <inheritdoc/>
    public ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        _entries.Enqueue(entry);
        return ValueTask.CompletedTask;
    }
}
