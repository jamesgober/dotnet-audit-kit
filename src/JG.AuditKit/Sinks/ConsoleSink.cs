using System.Text.Json;
using JG.AuditKit.Abstractions;

namespace JG.AuditKit.Sinks;

/// <summary>
/// Writes audit entries to standard output as JSON lines.
/// </summary>
public sealed class ConsoleSink : IAuditSink
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    public ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        string json = JsonSerializer.Serialize(entry, SerializerOptions);
        return new ValueTask(Console.Out.WriteLineAsync(json));
    }
}
