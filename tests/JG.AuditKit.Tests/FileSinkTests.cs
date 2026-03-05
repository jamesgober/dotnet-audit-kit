using System.Globalization;
using System.Text.Json;
using JG.AuditKit.Sinks;

namespace JG.AuditKit.Tests;

public sealed class FileSinkTests
{
    [Fact]
    public async Task WriteAsync_ValidEntry_WritesSingleJsonLine()
    {
        string directory = CreateTempDirectory();
        string path = Path.Combine(directory, "audit.jsonl");
        var sink = new FileSink(new FileSinkOptions { Path = path, RollDaily = false });

        await sink.WriteAsync(new AuditEntry("alice", "write", "users/1", DateTimeOffset.UtcNow, "corr"));

        string[] lines = await File.ReadAllLinesAsync(path);
        Assert.Single(lines);
        using JsonDocument document = JsonDocument.Parse(lines[0]);
        Assert.True(document.RootElement.TryGetProperty("actor", out _));
    }

    [Fact]
    public async Task WriteAsync_RollDailyEnabled_WritesToDatedFile()
    {
        string directory = CreateTempDirectory();
        string path = Path.Combine(directory, "audit.jsonl");
        var sink = new FileSink(new FileSinkOptions { Path = path, RollDaily = true });
        DateTimeOffset time = DateTimeOffset.Parse("2026-03-01T00:00:00Z", CultureInfo.InvariantCulture);

        await sink.WriteAsync(new AuditEntry("alice", "write", "users/1", time, "corr"));

        string expected = Path.Combine(directory, "audit-20260301.jsonl");
        Assert.True(File.Exists(expected));
    }

    [Fact]
    public async Task WriteAsync_MaxFileSizeExceeded_CreatesRotatedFile()
    {
        string directory = CreateTempDirectory();
        string path = Path.Combine(directory, "audit.jsonl");
        var sink = new FileSink(new FileSinkOptions { Path = path, RollDaily = false, MaxFileSizeBytes = 10 });

        await sink.WriteAsync(new AuditEntry("alice", "write", "users/1", DateTimeOffset.UtcNow, "corr"));
        await sink.WriteAsync(new AuditEntry("alice", "write", "users/2", DateTimeOffset.UtcNow, "corr"));

        string rotated = Path.Combine(directory, "audit.001.jsonl");
        Assert.True(File.Exists(path));
        Assert.True(File.Exists(rotated));
    }

    [Fact]
    public void Constructor_EmptyPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FileSink(new FileSinkOptions { Path = " " }));
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "JG.AuditKit.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
