namespace JG.AuditKit.Abstractions;

/// <summary>
/// Determines whether an audit entry should be recorded.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe.
/// </remarks>
public interface IAuditFilter
{
    /// <summary>
    /// Evaluates whether the specified entry should be written.
    /// </summary>
    /// <param name="entry">The audit entry candidate.</param>
    /// <returns><see langword="true"/> when the entry should be written; otherwise, <see langword="false"/>.</returns>
    bool ShouldWrite(AuditEntry entry);
}
