using System.Collections.ObjectModel;

namespace JG.AuditKit.Tests;

public sealed class AuditEntryTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesEntry()
    {
        var metadata = new Dictionary<string, string?> { ["ip"] = "127.0.0.1" };
        var timestamp = DateTimeOffset.UtcNow;

        var entry = new AuditEntry("alice", "write.user", "users/42", timestamp, "corr-1", metadata);

        Assert.Equal("alice", entry.Actor);
        Assert.Equal("write.user", entry.Action);
        Assert.Equal("users/42", entry.Resource);
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.Equal("corr-1", entry.CorrelationId);
        Assert.Equal("127.0.0.1", entry.Metadata["ip"]);
    }

    [Fact]
    public void Constructor_MetadataMutatedAfterCreation_EntryMetadataRemainsUnchanged()
    {
        var source = new Dictionary<string, string?> { ["reason"] = "create" };
        var entry = new AuditEntry("alice", "write.user", "users/42", DateTimeOffset.UtcNow, "corr", source);

        source["reason"] = "updated";

        Assert.Equal("create", entry.Metadata["reason"]);
    }

    [Fact]
    public void Constructor_InvalidActor_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new AuditEntry(" ", "write.user", "users/42", DateTimeOffset.UtcNow, "corr"));
    }

    [Fact]
    public void Constructor_WhitespaceMetadataKey_ThrowsArgumentException()
    {
        var metadata = new Dictionary<string, string?> { [" "] = "bad" };

        Assert.Throws<ArgumentException>(() =>
            new AuditEntry("alice", "write.user", "users/42", DateTimeOffset.UtcNow, "corr", metadata));
    }

    [Fact]
    public void MetadataProperty_IsReadOnlyDictionary()
    {
        var entry = new AuditEntry("alice", "write.user", "users/42", DateTimeOffset.UtcNow, "corr");

        Assert.IsType<ReadOnlyDictionary<string, string?>>(entry.Metadata);
    }
}
