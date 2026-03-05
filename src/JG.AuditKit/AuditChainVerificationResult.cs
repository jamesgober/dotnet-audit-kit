namespace JG.AuditKit;

/// <summary>
/// Represents the result of an audit hash chain verification run.
/// </summary>
public readonly struct AuditChainVerificationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditChainVerificationResult"/> struct.
    /// </summary>
    /// <param name="isValid">Indicates whether the chain is valid.</param>
    /// <param name="invalidIndex">The first invalid entry index, or <c>-1</c> when valid.</param>
    /// <param name="expectedHash">The expected hash value at the invalid entry.</param>
    /// <param name="actualHash">The actual hash value at the invalid entry.</param>
    public AuditChainVerificationResult(bool isValid, int invalidIndex, string? expectedHash, string? actualHash)
    {
        IsValid = isValid;
        InvalidIndex = invalidIndex;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
    }

    /// <summary>
    /// Gets a value indicating whether the chain is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the first invalid entry index, or <c>-1</c> when valid.
    /// </summary>
    public int InvalidIndex { get; }

    /// <summary>
    /// Gets the expected hash at the invalid index.
    /// </summary>
    public string? ExpectedHash { get; }

    /// <summary>
    /// Gets the actual hash at the invalid index.
    /// </summary>
    public string? ActualHash { get; }
}
