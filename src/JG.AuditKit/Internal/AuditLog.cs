using System.Threading.Channels;
using JG.AuditKit.Abstractions;
using Microsoft.Extensions.Logging;

namespace JG.AuditKit.Internal;

internal sealed class AuditLog : IAuditLog, IAsyncDisposable
{
    private readonly AuditKitOptions _options;
    private readonly IReadOnlyList<IAuditSink> _sinks;
    private readonly IReadOnlyList<IAuditFilter> _filters;
    private readonly ILogger<AuditLog> _logger;
    private readonly Channel<AuditEntry> _channel;
    private readonly CancellationTokenSource _shutdownTokenSource;
    private readonly Task _worker;
    private readonly object _chainLock = new();
    private string _lastHash;

    public AuditLog(
        AuditKitOptions options,
        IEnumerable<IAuditSink> sinks,
        IEnumerable<IAuditFilter> filters,
        ILogger<AuditLog> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(sinks);
        ArgumentNullException.ThrowIfNull(filters);
        ArgumentNullException.ThrowIfNull(logger);

        if (options.ChannelCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Channel capacity must be greater than zero.");
        }

        _options = options;
        _sinks = sinks as IReadOnlyList<IAuditSink> ?? sinks.ToArray();
        _filters = filters as IReadOnlyList<IAuditFilter> ?? filters.ToArray();
        _logger = logger;
        _lastHash = _options.HashSeed;
        _shutdownTokenSource = new CancellationTokenSource();

        foreach (IAuditSink sink in _sinks)
        {
            AuditKitLogMessages.SinkRegistered(_logger, sink.GetType().FullName ?? sink.GetType().Name);
        }

        foreach (IAuditFilter filter in _filters)
        {
            AuditKitLogMessages.FilterRegistered(_logger, filter.GetType().FullName ?? filter.GetType().Name);
        }

        if (!_options.EnableHashChaining)
        {
            AuditKitLogMessages.ChainDisabled(_logger);
        }

        var channelOptions = new BoundedChannelOptions(_options.ChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite,
            AllowSynchronousContinuations = false,
        };

        _channel = Channel.CreateBounded<AuditEntry>(channelOptions);
        _worker = Task.Run(ProcessQueueAsync);
    }

    public ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        AuditEntry normalized = entry.Timestamp == default
            ? entry with { Timestamp = _options.TimeProvider.GetUtcNow() }
            : entry;

        for (int i = 0; i < _filters.Count; i++)
        {
            if (!_filters[i].ShouldWrite(normalized))
            {
                return ValueTask.CompletedTask;
            }
        }

        if (_options.EnableHashChaining)
        {
            lock (_chainLock)
            {
                string previous = _lastHash;
                string hash = AuditHasher.ComputeHash(previous, normalized);
                normalized = normalized with { PreviousHash = previous, Hash = hash };
                _lastHash = hash;
            }
        }

        if (!_channel.Writer.TryWrite(normalized))
        {
            AuditKitLogMessages.QueueWriteDropped(_logger);
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        _shutdownTokenSource.Cancel();

        try
        {
            await _worker.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        _shutdownTokenSource.Dispose();
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            await foreach (AuditEntry entry in _channel.Reader.ReadAllAsync(_shutdownTokenSource.Token).ConfigureAwait(false))
            {
                await DispatchToSinksAsync(entry, _shutdownTokenSource.Token).ConfigureAwait(false);
            }

            AuditKitLogMessages.WorkerStopped(_logger);
        }
        catch (OperationCanceledException)
        {
            AuditKitLogMessages.WorkerStopped(_logger);
        }
        catch (Exception exception)
        {
            AuditKitLogMessages.WorkerTerminated(_logger, exception);
        }
    }

    private async Task DispatchToSinksAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        if (_sinks.Count == 0)
        {
            return;
        }

        var tasks = new Task[_sinks.Count];
        for (int i = 0; i < _sinks.Count; i++)
        {
            tasks[i] = WriteToSinkSafeAsync(_sinks[i], entry, cancellationToken);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task WriteToSinkSafeAsync(IAuditSink sink, AuditEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            await sink.WriteAsync(entry, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            AuditKitLogMessages.SinkWriteFailed(_logger, sink.GetType().FullName ?? sink.GetType().Name, exception);
        }
    }
}
