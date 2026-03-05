# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- _No changes yet._

## [1.0.0] - 2026-03-04

### Added
- Created `JG.AuditKit` .NET 8 library and test projects.
- Implemented immutable `AuditEntry` model with actor, action, resource, timestamp, correlation ID, metadata, and hash fields.
- Added abstractions: `IAuditLog`, `IAuditSink`, and `IAuditFilter`.
- Added `AuditKitOptions` and `FileSinkOptions` configuration models.
- Implemented channel-based internal `AuditLog` with async background dispatch and concurrent sink fan-out.
- Implemented SHA-256 hash chaining and `AuditHashChainVerifier` integrity validation API.
- Added built-in sinks: `ConsoleSink`, `FileSink`, and `InMemorySink`.
- Added dependency injection extensions: `AddAuditKit`, `AddAuditSink<T>`, `AddAuditFilter<T>`.
- Added source-generated `LoggerMessage` logging points for sink errors, chain validation failures, and lifecycle events.
- Added 30 xUnit tests covering creation, filtering, sink dispatch, failure isolation, concurrency, file sink behavior, hash verification, and DI registration.
- Added package metadata and NuGet packing configuration to `JG.AuditKit.csproj`.
- Added API documentation in `docs/API.md` and updated root `README.md` usage examples.

[Unreleased]: https://github.com/jamesgober/dotnet-audit-kit/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/jamesgober/dotnet-audit-kit/releases/tag/v1.0.0
