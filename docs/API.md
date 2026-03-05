# API Reference

## `AuditEntry`

Immutable audit event model.

### Constructor

- `AuditEntry(string actor, string action, string resource, DateTimeOffset timestamp, string correlationId, IReadOnlyDictionary<string, string?>? metadata = null, string? previousHash = null, string? hash = null)`

### Properties

- `Actor`
- `Action`
- `Resource`
- `Timestamp`
- `CorrelationId`
- `Metadata`
- `PreviousHash`
- `Hash`

## Abstractions

### `IAuditLog`

- `ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)`

### `IAuditSink`

- `ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)`

### `IAuditFilter`

- `bool ShouldWrite(AuditEntry entry)`

## Options

### `AuditKitOptions`

- `IList<Type> DefaultSinkTypes`
- `IList<Type> DefaultFilterTypes`
- `string HashSeed`
- `bool EnableHashChaining`
- `TimeProvider TimeProvider`
- `int ChannelCapacity`
- `FileSinkOptions FileSink`
- `void AddDefaultSink<TSink>() where TSink : class, IAuditSink`
- `void AddDefaultFilter<TFilter>() where TFilter : class, IAuditFilter`

### `FileSinkOptions`

- `string Path`
- `bool RollDaily`
- `long? MaxFileSizeBytes`

## Sinks

### `ConsoleSink`

Writes one JSON entry per line to standard output.

### `FileSink`

Appends one JSON entry per line to a file. Supports daily and size-based rotation.

### `InMemorySink`

Thread-safe in-memory sink for tests.

- `IReadOnlyList<AuditEntry> Entries`
- `int Count`

## Hash Chain Verification

### `AuditHashChainVerifier`

- `AuditChainVerificationResult Verify(IEnumerable<AuditEntry> entries, string seed, ILogger? logger = null)`

### `AuditChainVerificationResult`

- `bool IsValid`
- `int InvalidIndex`
- `string? ExpectedHash`
- `string? ActualHash`

## Service Registration

### `AuditKitServiceCollectionExtensions`

- `IServiceCollection AddAuditKit(this IServiceCollection services, Action<AuditKitOptions>? configure = null)`
- `IServiceCollection AddAuditSink<TSink>(this IServiceCollection services) where TSink : class, IAuditSink`
- `IServiceCollection AddAuditFilter<TFilter>(this IServiceCollection services) where TFilter : class, IAuditFilter`

## Example

```csharp
services.AddAuditKit(options =>
{
    options.DefaultSinkTypes.Clear();
    options.AddDefaultSink<FileSink>();
    options.EnableHashChaining = true;
    options.HashSeed = "seed";
    options.FileSink.Path = "audit.jsonl";
}).AddAuditFilter<MyFilter>();
