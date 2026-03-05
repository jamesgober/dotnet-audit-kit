using JG.AuditKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JG.AuditKit.Tests;

public sealed class AuditFilterTests
{
    [Fact]
    public async Task WriteAsync_FilterRejectsEntry_EntryIsNotWritten()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.DefaultFilterTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
            options.AddDefaultFilter<AlwaysDenyFilter>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<CollectingSink>().Single();

        await log.WriteAsync(new AuditEntry("alice", "write.user", "users/1", DateTimeOffset.UtcNow, "corr"));
        await Task.Delay(100);

        Assert.Empty(sink.Entries);
    }

    [Fact]
    public async Task WriteAsync_AllFiltersAllow_EntryIsWritten()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.DefaultFilterTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
            options.AddDefaultFilter<AlwaysAllowFilter>();
            options.AddDefaultFilter<ActionPrefixFilter>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<CollectingSink>().Single();

        await log.WriteAsync(new AuditEntry("alice", "write.user", "users/1", DateTimeOffset.UtcNow, "corr"));
        await TestWait.UntilAsync(() => sink.Entries.Count == 1, TimeSpan.FromSeconds(2));

        Assert.Single(sink.Entries);
    }

    [Fact]
    public async Task WriteAsync_FilterChecksActionPrefix_ReadActionIsSkipped()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.DefaultFilterTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
            options.AddDefaultFilter<ActionPrefixFilter>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();
        var log = provider.GetRequiredService<IAuditLog>();
        var sink = provider.GetServices<IAuditSink>().OfType<CollectingSink>().Single();

        await log.WriteAsync(new AuditEntry("alice", "read.user", "users/1", DateTimeOffset.UtcNow, "corr"));
        await Task.Delay(100);

        Assert.Empty(sink.Entries);
    }
}
