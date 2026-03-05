using JG.AuditKit;
using JG.AuditKit.Abstractions;
using JG.AuditKit.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration APIs for <c>JG.AuditKit</c>.
/// </summary>
public static class AuditKitServiceCollectionExtensions
{
    /// <summary>
    /// Registers core audit services and default sinks and filters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional options configuration callback.</param>
    /// <returns>The same service collection instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when configured default sink or filter types are invalid.</exception>
    public static IServiceCollection AddAuditKit(this IServiceCollection services, Action<AuditKitOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new AuditKitOptions();
        configure?.Invoke(options);

        ValidateOptions(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton(options.FileSink);
        services.TryAddSingleton<IAuditLog, AuditLog>();

        for (int i = 0; i < options.DefaultSinkTypes.Count; i++)
        {
            Type sinkType = options.DefaultSinkTypes[i];
            if (!typeof(IAuditSink).IsAssignableFrom(sinkType))
            {
                throw new InvalidOperationException($"Type '{sinkType.FullName}' must implement {nameof(IAuditSink)}.");
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IAuditSink), sinkType));
        }

        for (int i = 0; i < options.DefaultFilterTypes.Count; i++)
        {
            Type filterType = options.DefaultFilterTypes[i];
            if (!typeof(IAuditFilter).IsAssignableFrom(filterType))
            {
                throw new InvalidOperationException($"Type '{filterType.FullName}' must implement {nameof(IAuditFilter)}.");
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IAuditFilter), filterType));
        }

        return services;
    }

    /// <summary>
    /// Registers a custom audit sink.
    /// </summary>
    /// <typeparam name="TSink">The sink type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddAuditSink<TSink>(this IServiceCollection services)
        where TSink : class, IAuditSink
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuditSink, TSink>());
        return services;
    }

    /// <summary>
    /// Registers a custom audit filter.
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddAuditFilter<TFilter>(this IServiceCollection services)
        where TFilter : class, IAuditFilter
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuditFilter, TFilter>());
        return services;
    }

    private static void ValidateOptions(AuditKitOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.HashSeed))
        {
            throw new ArgumentException("Hash seed is required.", nameof(options));
        }

        if (options.TimeProvider is null)
        {
            throw new ArgumentException("Time provider is required.", nameof(options));
        }

        if (options.ChannelCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Channel capacity must be greater than zero.");
        }
    }
}
