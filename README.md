# dotnet-audit-kit

[![NuGet](https://img.shields.io/nuget/v/JG.AuditKit?logo=nuget)](https://www.nuget.org/packages/JG.AuditKit)
[![Downloads](https://img.shields.io/nuget/dt/JG.AuditKit?color=%230099ff&logo=nuget)](https://www.nuget.org/packages/JG.AuditKit)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](./LICENSE)
[![CI](https://github.com/jamesgober/dotnet-audit-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/jamesgober/dotnet-audit-kit/actions)

---

Immutable audit logging for .NET applications. Record who did what, when, and where with tamper-evident hash chains, pluggable sinks, and non-blocking background flushing. Built for compliance patterns (SOC 2, HIPAA, PCI-DSS).

## Features

- **Structured audit entries** — actor, action, resource, timestamp, correlation ID, and custom metadata
- **Tamper-evident hash chains** — SHA-256 linked entries detect modification or deletion
- **Pluggable sinks** — file (JSON lines), console, or custom `IAuditSink` implementations
- **Background flushing** — `Channel<T>`-based async pipeline, never blocks the request path
- **Batch writing** — configurable batch size and flush interval for throughput
- **Chain verification** — replay from seed to verify integrity of the entire audit trail
- **Date-based file rolling** — automatic log rotation for file sinks
- **Query support** — filter by date range, actor, action, or resource pattern

## Installation

```bash
dotnet add package JG.AuditKit
```

## Quick Start

```csharp
builder.Services.AddAuditKit(options =>
{
    options.AddFileSink("audit.jsonl", rollByDate: true);
    options.EnableHashChain = true;
    options.FlushInterval = TimeSpan.FromSeconds(5);
});

// Record an audit event
await auditLog.RecordAsync(new AuditEntry
{
    Actor = userId,
    Action = "user.updated",
    Resource = $"users/{targetUserId}",
    Metadata = new { Field = "email", OldValue = old, NewValue = updated }
});
```

## Documentation

- **[API Reference](./docs/API.md)** — Full API documentation and examples

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

Licensed under the Apache License 2.0. See [LICENSE](./LICENSE) for details.

---

**Ready to get started?** Install via NuGet and check out the [API reference](./docs/API.md).
