using JG.AuditKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JG.AuditKit.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public async Task AddAuditKit_RegistersAuditLog()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditKit(options =>
        {
            options.DefaultSinkTypes.Clear();
            options.AddDefaultSink<CollectingSink>();
        });

        await using ServiceProvider provider = services.BuildServiceProvider();

        var auditLog = provider.GetService<IAuditLog>();

        Assert.NotNull(auditLog);
    }

    [Fact]
    public async Task AddAuditSink_RegistersSink()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditSink<CollectingSink>();
        services.AddAuditKit(options => options.DefaultSinkTypes.Clear());

        await using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IAuditSink>(), sink => sink is CollectingSink);
    }

    [Fact]
    public async Task AddAuditFilter_RegistersFilter()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuditFilter<AlwaysAllowFilter>();
        services.AddAuditKit(options => options.DefaultSinkTypes.Clear());

        await using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Contains(provider.GetServices<IAuditFilter>(), filter => filter is AlwaysAllowFilter);
    }

    [Fact]
    public void AddAuditKit_InvalidHashSeed_ThrowsArgumentException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() =>
            services.AddAuditKit(options => options.HashSeed = " "));
    }
}
