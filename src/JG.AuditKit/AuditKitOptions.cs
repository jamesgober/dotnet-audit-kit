using JG.AuditKit.Abstractions;
using JG.AuditKit.Sinks;

namespace JG.AuditKit;

/// <summary>
/// Configures <c>JG.AuditKit</c> behavior.
/// </summary>
public sealed class AuditKitOptions
{
    /// <summary>
    /// Gets the default sink service types that are registered by <c>AddAuditKit</c>.
    /// </summary>
    public IList<Type> DefaultSinkTypes { get; } = new List<Type> { typeof(ConsoleSink) };

    /// <summary>
    /// Gets the default filter service types that are registered by <c>AddAuditKit</c>.
    /// </summary>
    public IList<Type> DefaultFilterTypes { get; } = new List<Type>();

    /// <summary>
    /// Gets or sets the hash seed for the first chain entry.
    /// </summary>
    public string HashSeed { get; set; } = "JG.AuditKit.Seed";

    /// <summary>
    /// Gets or sets a value indicating whether hash chaining is enabled.
    /// </summary>
    public bool EnableHashChaining { get; set; } = true;

    /// <summary>
    /// Gets or sets the clock used to resolve timestamps.
    /// </summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>
    /// Gets or sets the channel capacity used by the internal audit queue.
    /// </summary>
    public int ChannelCapacity { get; set; } = 4096;

    /// <summary>
    /// Gets file sink configuration used by <see cref="FileSink"/>.
    /// </summary>
    public FileSinkOptions FileSink { get; } = new();

    /// <summary>
    /// Adds a default sink type that is auto-registered.
    /// </summary>
    /// <typeparam name="TSink">The sink implementation type.</typeparam>
    public void AddDefaultSink<TSink>()
        where TSink : class, IAuditSink
    {
        DefaultSinkTypes.Add(typeof(TSink));
    }

    /// <summary>
    /// Adds a default filter type that is auto-registered.
    /// </summary>
    /// <typeparam name="TFilter">The filter implementation type.</typeparam>
    public void AddDefaultFilter<TFilter>()
        where TFilter : class, IAuditFilter
    {
        DefaultFilterTypes.Add(typeof(TFilter));
    }
}
