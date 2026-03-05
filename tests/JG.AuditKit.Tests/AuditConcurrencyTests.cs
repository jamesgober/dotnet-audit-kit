using System.Globalization;
using JG.AuditKit.Abstractions;
using JG.AuditKit.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace JG.AuditKit.Tests;

public sealed class AuditConcurrencyTests
{
    [Fact]
    public async Task WriteAsync_ConcurrentCalls_AllEntriesWritten()
    {
        const int count = 500;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<InMemorySink>();
            options.ChannelCapacity = 2048;
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<InMemorySink>().Single();

        Task[] writes = new Task[count];
        for (int i = 0; i < count; i++)
        {
            int index = i;
            writes[i] = log.WriteAsync(new AuditEntry("alice", "write.user", $"users/{index}", DateTimeOffset.UtcNow, "corr")).AsTask();
        }

        await Task.WhenAll(writes);
        await TestWait.UntilAsync(() => sink.Count == count, TimeSpan.FromSeconds(5));

        Assert.Equal(count, sink.Count);
    }

    [Fact]
    public async Task WriteAsync_ConcurrentCalls_HashesAreAssigned()
    {
        const int count = 200;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<InMemorySink>();
            options.HashSeed = "seed";
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<InMemorySink>().Single();

        await Task.WhenAll(
            Enumerable.Range(0, count)
                .Select(i => log.WriteAsync(new AuditEntry("alice", "write", $"res/{i}", DateTimeOffset.UtcNow, "corr")).AsTask()))
            ;

        await TestWait.UntilAsync(() => sink.Count == count, TimeSpan.FromSeconds(5));

        Assert.All(sink.Entries, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.PreviousHash));
            Assert.False(string.IsNullOrWhiteSpace(entry.Hash));
        });
    }

    [Fact]
    public async Task WriteAsync_ConcurrentCalls_ChainRemainsValid()
    {
        const int count = 250;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<InMemorySink>();
            options.HashSeed = "seed";
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<InMemorySink>().Single();

        var tasks = new List<Task>(count);
        for (int i = 0; i < count; i++)
        {
            tasks.Add(log.WriteAsync(new AuditEntry("actor", "write", $"obj/{i}", DateTimeOffset.UtcNow, "corr")).AsTask());
        }

        await Task.WhenAll(tasks);
        await TestWait.UntilAsync(() => sink.Count == count, TimeSpan.FromSeconds(5));

        AuditChainVerificationResult result = AuditHashChainVerifier.Verify(sink.Entries, "seed");
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task WriteAsync_DefaultTimestamp_UsesConfiguredTimeProvider()
    {
        DateTimeOffset expectedTime = DateTimeOffset.Parse("2026-02-01T05:00:00Z", CultureInfo.InvariantCulture);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<InMemorySink>();
            options.TimeProvider = new FixedTimeProvider(expectedTime);
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<InMemorySink>().Single();

        await log.WriteAsync(new AuditEntry("alice", "write", "obj/1", default, "corr"));
        await TestWait.UntilAsync(() => sink.Count == 1, TimeSpan.FromSeconds(2));

        Assert.Equal(expectedTime, sink.Entries[0].Timestamp);
    }
}
