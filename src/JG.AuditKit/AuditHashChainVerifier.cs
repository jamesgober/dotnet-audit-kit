using JG.AuditKit.Internal;
using Microsoft.Extensions.Logging;

namespace JG.AuditKit;

/// <summary>
/// Verifies SHA-256 hash chain integrity for a sequence of audit entries.
/// </summary>
public static class AuditHashChainVerifier
{
    /// <summary>
    /// Verifies chain integrity from the specified seed hash.
    /// </summary>
    /// <param name="entries">The entries to verify in order.</param>
    /// <param name="seed">The initial hash seed.</param>
    /// <param name="logger">An optional logger for chain break diagnostics.</param>
    /// <returns>The chain verification result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> or <paramref name="seed"/> is <see langword="null"/>.</exception>
    public static AuditChainVerificationResult Verify(IEnumerable<AuditEntry> entries, string seed, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(seed);

        string previous = seed;
        int index = 0;

        foreach (AuditEntry entry in entries)
        {
            ArgumentNullException.ThrowIfNull(entry);
            string expected = AuditHasher.ComputeHash(previous, entry with { Hash = null, PreviousHash = null });
            string actual = entry.Hash ?? string.Empty;
            string actualPrevious = entry.PreviousHash ?? string.Empty;

            if (!AuditHasher.HashesEqual(previous, actualPrevious) || !AuditHasher.HashesEqual(expected, actual))
            {
                if (logger is not null)
                {
                    AuditKitLogMessages.ChainValidationFailed(logger, index, expected, actual);
                }

                return new AuditChainVerificationResult(false, index, expected, actual);
            }

            previous = actual;
            index++;
        }

        return new AuditChainVerificationResult(true, -1, null, null);
    }
}
