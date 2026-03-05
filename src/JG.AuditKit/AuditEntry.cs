using System.Collections.ObjectModel;

namespace JG.AuditKit;

/// <summary>
/// Represents one immutable audit event.
/// </summary>
public sealed record AuditEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditEntry"/> class.
    /// </summary>
    /// <param name="actor">The actor who performed the action.</param>
    /// <param name="action">The action that occurred.</param>
    /// <param name="resource">The resource that was targeted.</param>
    /// <param name="timestamp">The event timestamp.</param>
    /// <param name="correlationId">The correlation identifier for tracing.</param>
    /// <param name="metadata">Additional key/value metadata.</param>
    /// <param name="previousHash">The previous chain hash.</param>
    /// <param name="hash">The current entry chain hash.</param>
    /// <exception cref="ArgumentException">Thrown when required string values are empty.</exception>
    public AuditEntry(
        string actor,
        string action,
        string resource,
        DateTimeOffset timestamp,
        string correlationId,
        IReadOnlyDictionary<string, string?>? metadata = null,
        string? previousHash = null,
        string? hash = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new ArgumentException("Actor is required.", nameof(actor));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException("Resource is required.", nameof(resource));
        }

        Actor = actor;
        Action = action;
        Resource = resource;
        Timestamp = timestamp;
        CorrelationId = correlationId ?? string.Empty;
        Metadata = new ReadOnlyDictionary<string, string?>(CopyMetadata(metadata));
        PreviousHash = previousHash;
        Hash = hash;
    }

    /// <summary>
    /// Gets the actor who performed the action.
    /// </summary>
    public string Actor { get; init; }

    /// <summary>
    /// Gets the action that occurred.
    /// </summary>
    public string Action { get; init; }

    /// <summary>
    /// Gets the target resource.
    /// </summary>
    public string Resource { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the correlation identifier for request or workflow tracing.
    /// </summary>
    public string CorrelationId { get; init; }

    /// <summary>
    /// Gets additional context metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Metadata { get; init; }

    /// <summary>
    /// Gets the hash of the previous entry in the chain.
    /// </summary>
    public string? PreviousHash { get; init; }

    /// <summary>
    /// Gets the hash for this entry.
    /// </summary>
    public string? Hash { get; init; }

    private static Dictionary<string, string?> CopyMetadata(IReadOnlyDictionary<string, string?>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return new Dictionary<string, string?>(StringComparer.Ordinal);
        }

        var copy = new Dictionary<string, string?>(metadata.Count, StringComparer.Ordinal);
        foreach (KeyValuePair<string, string?> pair in metadata)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                throw new ArgumentException("Metadata keys must not be null, empty, or whitespace.", nameof(metadata));
            }

            copy[pair.Key] = pair.Value;
        }

        return copy;
    }
}
