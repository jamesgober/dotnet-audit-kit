using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace JG.AuditKit.Internal;

internal static class AuditHasher
{
    public static string ComputeHash(string previousHash, AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        string payload = BuildPayload(previousHash, entry);
        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        byte[] hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    public static bool HashesEqual(string? left, string? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        byte[] leftBytes = Encoding.UTF8.GetBytes(left);
        byte[] rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string BuildPayload(string previousHash, AuditEntry entry)
    {
        var builder = new StringBuilder(256);
        builder.Append(previousHash);
        builder.Append('|');
        builder.Append(entry.Actor);
        builder.Append('|');
        builder.Append(entry.Action);
        builder.Append('|');
        builder.Append(entry.Resource);
        builder.Append('|');
        builder.Append(entry.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
        builder.Append('|');
        builder.Append(entry.CorrelationId);

        if (entry.Metadata.Count == 0)
        {
            return builder.ToString();
        }

        string[] keys = new string[entry.Metadata.Count];
        int i = 0;
        foreach (KeyValuePair<string, string?> pair in entry.Metadata)
        {
            keys[i++] = pair.Key;
        }

        Array.Sort(keys, StringComparer.Ordinal);

        for (i = 0; i < keys.Length; i++)
        {
            string key = keys[i];
            _ = entry.Metadata.TryGetValue(key, out string? value);
            builder.Append('|');
            builder.Append(key);
            builder.Append('=');
            builder.Append(value);
        }

        return builder.ToString();
    }
}
