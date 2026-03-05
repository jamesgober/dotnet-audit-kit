using System.Threading;
using System.Threading.Tasks;

namespace JG.AuditKit.Abstractions;

/// <summary>
/// Writes immutable audit entries to the configured audit pipeline.
/// </summary>
/// <remarks>
/// Implementations are thread-safe and designed for concurrent use across multiple request threads.
/// </remarks>
public interface IAuditLog
{
    /// <summary>
    /// Enqueues an audit entry for asynchronous processing.
    /// </summary>
    /// <param name="entry">The audit entry to write.</param>
    /// <param name="cancellationToken">A cancellation token that can stop enqueueing.</param>
    /// <returns>A task that completes when the entry is accepted or skipped by filters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is <see langword="null"/>.</exception>
    ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
