using System.Globalization;
using System.Text.Json;
using JG.AuditKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JG.AuditKit.Tests;

public sealed class AuditHashChainTests
{
    [Fact]
    public async Task Verify_ValidChain_ReturnsValid()
    {
        IReadOnlyList<AuditEntry> entries = await WriteEntriesAsync(3, enableHash: true);

        AuditChainVerificationResult result = AuditHashChainVerifier.Verify(entries, "seed");

        Assert.True(result.IsValid);
        Assert.Equal(-1, result.InvalidIndex);
    }

    [Fact]
    public async Task Verify_TamperedEntry_ReturnsInvalid()
    {
        IReadOnlyList<AuditEntry> entries = await WriteEntriesAsync(3, enableHash: true);
        AuditEntry[] copy = entries.ToArray();
        copy[1] = copy[1] with { Resource = "users/999" };

        AuditChainVerificationResult result = AuditHashChainVerifier.Verify(copy, "seed");

        Assert.False(result.IsValid);
        Assert.Equal(1, result.InvalidIndex);
    }

    [Fact]
    public async Task Verify_MissingPreviousHash_ReturnsInvalid()
    {
        IReadOnlyList<AuditEntry> entries = await WriteEntriesAsync(2, enableHash: true);
        AuditEntry[] copy = entries.ToArray();
        copy[0] = copy[0] with { PreviousHash = null };

        AuditChainVerificationResult result = AuditHashChainVerifier.Verify(copy, "seed");

        Assert.False(result.IsValid);
        Assert.Equal(0, result.InvalidIndex);
    }

    [Fact]
    public async Task WriteAsync_EnableHashing_PopulatesHashFields()
    {
        IReadOnlyList<AuditEntry> entries = await WriteEntriesAsync(1, enableHash: true);

        Assert.False(string.IsNullOrEmpty(entries[0].PreviousHash));
        Assert.False(string.IsNullOrEmpty(entries[0].Hash));
    }

    [Fact]
    public async Task WriteAsync_DisableHashing_LeavesHashFieldsNull()
    {
        IReadOnlyList<AuditEntry> entries = await WriteEntriesAsync(1, enableHash: false);

        Assert.Null(entries[0].PreviousHash);
        Assert.Null(entries[0].Hash);
    }

    [Fact]
    public void AuditEntry_SerializesToJsonWithoutCustomConverters()
    {
        var entry = new AuditEntry(
            "alice",
            "write.user",
            "users/42",
            DateTimeOffset.Parse("2026-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            "corr",
            new Dictionary<string, string?> { ["why"] = "support-ticket" },
            "prev",
            "hash");

        string json = JsonSerializer.Serialize(entry);

        Assert.Contains("\"actor\":", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"metadata\":", json, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<IReadOnlyList<AuditEntry>> WriteEntriesAsync(int count, bool enableHash)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditSink<CollectingSink>();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
            options.HashSeed = "seed";
            options.EnableHashChaining = enableHash;
            options.ChannelCapacity = 1024;
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetRequiredService<IEnumerable<IAuditSink>>().OfType<CollectingSink>().Single();

        for (int i = 0; i < count; i++)
        {
            await log.WriteAsync(new AuditEntry("alice", $"write.{i}", $"users/{i}", DateTimeOffset.UtcNow, "corr"));
        }

        await TestWait.UntilAsync(() => sink.Entries.Count == count, TimeSpan.FromSeconds(3));
        return sink.Entries;
    }
}
