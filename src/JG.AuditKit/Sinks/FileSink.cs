using System.Globalization;
using System.Text;
using System.Text.Json;
using JG.AuditKit.Abstractions;

namespace JG.AuditKit.Sinks;

/// <summary>
/// Appends audit entries to a JSON lines file.
/// </summary>
/// <remarks>
/// Writes are serialized with an async lock to preserve line-level append ordering.
/// </remarks>
public sealed class FileSink : IAuditSink, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly FileSinkOptions _options;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSink"/> class.
    /// </summary>
    /// <param name="options">The file sink options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when configured path is empty.</exception>
    public FileSink(FileSinkOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Path))
        {
            throw new ArgumentException("File sink path is required.", nameof(options));
        }

        if (options.MaxFileSizeBytes is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Max file size must be greater than zero.");
        }

        _options = options;
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            string targetPath = ResolvePath(entry.Timestamp == default ? DateTimeOffset.UtcNow : entry.Timestamp);
            string? directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(entry, SerializerOptions);
            byte[] bytes = Encoding.UTF8.GetBytes(json + Environment.NewLine);

            await using var stream = new FileStream(
                targetPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Releases sink resources.
    /// </summary>
    public void Dispose()
    {
        _writeLock.Dispose();
    }

    private string ResolvePath(DateTimeOffset timestamp)
    {
        string path = _options.Path;

        if (_options.RollDaily)
        {
            string directory = Path.GetDirectoryName(path) ?? string.Empty;
            string extension = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string dateToken = timestamp.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string dailyName = string.IsNullOrEmpty(extension)
                ? $"{fileName}-{dateToken}"
                : $"{fileName}-{dateToken}{extension}";
            path = string.IsNullOrEmpty(directory)
                ? dailyName
                : Path.Combine(directory, dailyName);
        }

        long? maxBytes = _options.MaxFileSizeBytes;
        if (maxBytes is null)
        {
            return path;
        }

        if (!File.Exists(path))
        {
            return path;
        }

        var info = new FileInfo(path);
        if (info.Length < maxBytes.Value)
        {
            return path;
        }

        string directoryName = Path.GetDirectoryName(path) ?? string.Empty;
        string extensionName = Path.GetExtension(path);
        string baseName = Path.GetFileNameWithoutExtension(path);
        int index = 1;

        while (true)
        {
            string rotatedName = string.IsNullOrEmpty(extensionName)
                ? $"{baseName}.{index:D3}"
                : $"{baseName}.{index:D3}{extensionName}";
            string rotatedPath = string.IsNullOrEmpty(directoryName)
                ? rotatedName
                : Path.Combine(directoryName, rotatedName);

            if (!File.Exists(rotatedPath))
            {
                return rotatedPath;
            }

            var rotatedInfo = new FileInfo(rotatedPath);
            if (rotatedInfo.Length < maxBytes.Value)
            {
                return rotatedPath;
            }

            index++;
        }
    }
}
