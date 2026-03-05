namespace JG.AuditKit.Sinks;

/// <summary>
/// Configures file sink behavior.
/// </summary>
public sealed class FileSinkOptions
{
    /// <summary>
    /// Gets or sets the base path for audit output.
    /// </summary>
    public string Path { get; set; } = "audit.jsonl";

    /// <summary>
    /// Gets or sets a value indicating whether to roll files daily.
    /// </summary>
    public bool RollDaily { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum file size in bytes before rotation. Set to <c>null</c> to disable size-based rotation.
    /// </summary>
    public long? MaxFileSizeBytes { get; set; }
}
