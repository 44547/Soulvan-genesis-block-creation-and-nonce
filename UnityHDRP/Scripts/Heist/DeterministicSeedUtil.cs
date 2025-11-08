using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// DeterministicSeedUtil: Utilities for generating deterministic replay seeds
/// with server-side HMAC signing for provenance and verification.
/// Client-side only computes local digest; server provides authoritative signature.
/// </summary>
public static class DeterministicSeedUtil
{
    /// <summary>
    /// Compose seed input string from mission parameters
    /// Format: missionId|timestamp|modules|contributorId
    /// </summary>
    /// <param name="missionId">Unique mission identifier</param>
    /// <param name="timestamp">Unix timestamp or Time.time snapshot</param>
    /// <param name="modules">Array of activated module IDs</param>
    /// <param name="contributorId">Contributor wallet or ID</param>
    /// <returns>Concatenated seed input string</returns>
    public static string ComposeSeedInput(string missionId, long timestamp, string[] modules, string contributorId)
    {
        var modulesConcat = string.Join(",", modules ?? new string[0]);
        return $"{missionId}|{timestamp}|{modulesConcat}|{contributorId}";
    }

    /// <summary>
    /// Compute local SHA256 digest of seed input (client-side only)
    /// This creates a deterministic hash for replay identification.
    /// Server must provide HMAC signature for verification.
    /// </summary>
    /// <param name="seedInput">Seed input string from ComposeSeedInput</param>
    /// <returns>Lowercase hex string of SHA256 hash</returns>
    public static string ComputeLocalDigest(string seedInput)
    {
        if (string.IsNullOrEmpty(seedInput))
        {
            throw new ArgumentException("Seed input cannot be null or empty", nameof(seedInput));
        }

        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(seedInput);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Compose signed seed token with server signature
    /// Format: digest:serverSignature
    /// </summary>
    /// <param name="digest">Local digest from ComputeLocalDigest</param>
    /// <param name="serverSignature">HMAC-SHA256 signature from server</param>
    /// <returns>Signed seed token string</returns>
    public static string ComposeSignedSeed(string digest, string serverSignature)
    {
        if (string.IsNullOrEmpty(digest))
        {
            throw new ArgumentException("Digest cannot be null or empty", nameof(digest));
        }

        if (string.IsNullOrEmpty(serverSignature))
        {
            throw new ArgumentException("Server signature cannot be null or empty", nameof(serverSignature));
        }

        return $"{digest}:{serverSignature}";
    }

    /// <summary>
    /// Parse signed seed token into digest and signature components
    /// </summary>
    /// <param name="signedSeed">Signed seed token from ComposeSignedSeed</param>
    /// <param name="digest">Output: extracted digest</param>
    /// <param name="serverSignature">Output: extracted server signature</param>
    /// <returns>True if parsing succeeded</returns>
    public static bool ParseSignedSeed(string signedSeed, out string digest, out string serverSignature)
    {
        digest = null;
        serverSignature = null;

        if (string.IsNullOrEmpty(signedSeed))
        {
            return false;
        }

        var parts = signedSeed.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        digest = parts[0];
        serverSignature = parts[1];
        return true;
    }

    /// <summary>
    /// Verify server signature using server-side verification endpoint
    /// Client cannot verify HMAC locally without secret; must call server.
    /// </summary>
    /// <param name="digest">Local digest</param>
    /// <param name="serverSignature">Server-provided HMAC signature</param>
    /// <param name="serverVerifier">Function to call server verification endpoint</param>
    /// <returns>True if signature is valid</returns>
    public static bool VerifyServerSignature(string digest, string serverSignature, Func<string, bool> serverVerifier)
    {
        if (string.IsNullOrEmpty(digest) || string.IsNullOrEmpty(serverSignature))
        {
            return false;
        }

        if (serverVerifier == null)
        {
            throw new ArgumentNullException(nameof(serverVerifier), "Server verifier function cannot be null");
        }

        string signedSeed = ComposeSignedSeed(digest, serverSignature);
        return serverVerifier.Invoke(signedSeed);
    }

    /// <summary>
    /// Generate a short replay ID from digest (first 8 characters)
    /// Useful for UI display and quick reference
    /// </summary>
    /// <param name="digest">Full digest string</param>
    /// <returns>Short replay ID (8 chars)</returns>
    public static string GenerateShortReplayId(string digest)
    {
        if (string.IsNullOrEmpty(digest) || digest.Length < 8)
        {
            return "UNKNOWN";
        }

        return digest.Substring(0, 8).ToUpperInvariant();
    }

    /// <summary>
    /// Compute timestamp from DateTime (Unix milliseconds)
    /// </summary>
    /// <param name="dateTime">DateTime to convert</param>
    /// <returns>Unix timestamp in milliseconds</returns>
    public static long ComputeTimestamp(DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    /// <summary>
    /// Compute current timestamp (Unix milliseconds)
    /// </summary>
    /// <returns>Current Unix timestamp in milliseconds</returns>
    public static long ComputeCurrentTimestamp()
    {
        return ComputeTimestamp(DateTime.UtcNow);
    }

    /// <summary>
    /// Validate seed input format
    /// </summary>
    /// <param name="seedInput">Seed input string to validate</param>
    /// <returns>True if format is valid</returns>
    public static bool ValidateSeedInput(string seedInput)
    {
        if (string.IsNullOrEmpty(seedInput))
        {
            return false;
        }

        // Expected format: missionId|timestamp|modules|contributorId
        var parts = seedInput.Split('|');
        return parts.Length == 4;
    }

    /// <summary>
    /// Create deterministic seed for testing/debugging
    /// Uses fixed timestamp and modules for reproducibility
    /// </summary>
    /// <param name="missionId">Mission ID</param>
    /// <param name="contributorId">Contributor ID</param>
    /// <returns>Deterministic seed input string</returns>
    public static string CreateTestSeedInput(string missionId, string contributorId)
    {
        long fixedTimestamp = 1700000000000; // Fixed timestamp
        string[] fixedModules = new string[] { "test:module1", "test:module2" };
        return ComposeSeedInput(missionId, fixedTimestamp, fixedModules, contributorId);
    }
}
