using JG.AuditKit.Abstractions;
using JG.AuditKit.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace JG.AuditKit.Tests;

public sealed class AuditSinkDispatchTests
{
    [Fact]
    public async Task WriteAsync_MultipleSinks_AllReceiveEntry()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
            options.AddDefaultSink<InMemorySink>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var collecting = provider.GetServices<IAuditSink>().OfType<CollectingSink>().Single();
        var memory = provider.GetServices<IAuditSink>().OfType<InMemorySink>().Single();

        await log.WriteAsync(new AuditEntry("alice", "write.user", "users/1", DateTimeOffset.UtcNow, "corr"));

        await TestWait.UntilAsync(() => collecting.Entries.Count == 1 && memory.Count == 1, TimeSpan.FromSeconds(2));

        Assert.Single(collecting.Entries);
        Assert.Single(memory.Entries);
    }

    [Fact]
    public async Task WriteAsync_SinkThrows_OtherSinkStillReceivesEntry()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<ThrowingSink>();
            options.AddDefaultSink<CollectingSink>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var collecting = provider.GetServices<IAuditSink>().OfType<CollectingSink>().Single();

        await log.WriteAsync(new AuditEntry("alice", "write.user", "users/1", DateTimeOffset.UtcNow, "corr"));
        await TestWait.UntilAsync(() => collecting.Entries.Count == 1, TimeSpan.FromSeconds(2));

        Assert.Single(collecting.Entries);
    }

    [Fact]
    public async Task WriteAsync_SlowSink_DoesNotBlockCaller()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<SlowSink>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();

        DateTime start = DateTime.UtcNow;
        await log.WriteAsync(new AuditEntry("alice", "write.user", "users/1", DateTimeOffset.UtcNow, "corr"));
        TimeSpan elapsed = DateTime.UtcNow - start;

        Assert.True(elapsed < TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task WriteAsync_CancellationRequested_ReturnsCanceledTask()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await log.WriteAsync(new AuditEntry("alice", "write.user", "users/1", DateTimeOffset.UtcNow, "corr"), cts.Token).AsTask());
    }
}
