# dotnet-audit-kit

[![NuGet](https://img.shields.io/nuget/v/JG.AuditKit?logo=nuget)](https://www.nuget.org/packages/JG.AuditKit)
[![Downloads](https://img.shields.io/nuget/dt/JG.AuditKit?color=%230099ff&logo=nuget)](https://www.nuget.org/packages/JG.AuditKit)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](./LICENSE)
[![CI](https://github.com/jamesgober/dotnet-audit-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/jamesgober/dotnet-audit-kit/actions)

---

Immutable audit logging for .NET 8 applications. Record who did what, when, where, and why with tamper-evident hash chains, filter pipelines, and non-blocking sink dispatch.

## Features

- Immutable `AuditEntry` model
- SHA-256 hash chaining with configurable seed
- Built-in sinks: `ConsoleSink`, `FileSink`, `InMemorySink`
- Pluggable `IAuditSink` and `IAuditFilter`
- Async `Channel<T>` dispatch pipeline that keeps request paths non-blocking
- Sink failure isolation (one sink failure does not stop other sinks)
- Hash chain verification API
- DI extensions: `AddAuditKit`, `AddAuditSink<T>`, `AddAuditFilter<T>`

## Installation

```bash
dotnet add package JG.AuditKit
```

## Quick Start

```csharp
using JG.AuditKit;
using JG.AuditKit.Abstractions;
using JG.AuditKit.Sinks;

builder.Services
    .AddAuditKit(options =>
    {
        options.DefaultSinkTypes.Clear();
        options.AddDefaultSink<FileSink>();
        options.EnableHashChaining = true;
        options.HashSeed = "my-seed";
        options.FileSink.Path = "audit.jsonl";
        options.FileSink.RollDaily = true;
    })
    .AddAuditFilter<MyAuditFilter>();

// ...

var auditLog = app.Services.GetRequiredService<IAuditLog>();

await auditLog.WriteAsync(new AuditEntry(
    actor: "user:42",
    action: "write.order",
    resource: "orders/1001",
    timestamp: DateTimeOffset.UtcNow,
    correlationId: "corr-123",
    metadata: new Dictionary<string, string?>
    {
        ["ip"] = "10.0.0.10",
        ["reason"] = "user-update"
    }));
```

## Documentation

- [API Reference](./docs/API.md)

## License

Licensed under the Apache License 2.0. See [LICENSE](./LICENSE) for details.

---

**Ready to get started?** Install via NuGet and check out the [API reference](./docs/API.md).
