using Microsoft.Extensions.Logging;

namespace JG.AuditKit.Internal;

internal static partial class AuditKitLogMessages
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Audit sink '{sinkType}' failed while writing entry.")]
    public static partial void SinkWriteFailed(ILogger logger, string sinkType, Exception exception);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Audit hash chain validation failed at index {index}. Expected '{expectedHash}' and found '{actualHash}'.")]
    public static partial void ChainValidationFailed(ILogger logger, int index, string expectedHash, string actualHash);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Audit hash chaining is disabled.")]
    public static partial void ChainDisabled(ILogger logger);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Audit sink registered: {sinkType}")]
    public static partial void SinkRegistered(ILogger logger, string sinkType);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Audit filter registered: {filterType}")]
    public static partial void FilterRegistered(ILogger logger, string filterType);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning, Message = "Audit queue dropped an entry because the writer is closed.")]
    public static partial void QueueWriteDropped(ILogger logger);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Debug, Message = "Audit worker stopped.")]
    public static partial void WorkerStopped(ILogger logger);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Error, Message = "Audit worker terminated unexpectedly.")]
    public static partial void WorkerTerminated(ILogger logger, Exception exception);
}
