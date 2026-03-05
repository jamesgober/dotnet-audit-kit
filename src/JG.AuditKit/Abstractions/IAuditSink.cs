using System.Threading;
using System.Threading.Tasks;

namespace JG.AuditKit.Abstractions;

/// <summary>
/// Writes accepted audit entries to a storage destination.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe and must not throw for recoverable write failures.
/// </remarks>
public interface IAuditSink
{
    /// <summary>
    /// Writes a single audit entry to the sink.
    /// </summary>
    /// <param name="entry">The audit entry.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that represents the write operation.</returns>
    ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
