using System.Security.Cryptography;

namespace Asus.AutoUpdate
{
    /// <summary>Outcome of a pending-update check at startup.</summary>
    public enum PendingUpdateAction
    {
        /// <summary>No update was in flight — nothing to do.</summary>
        None,
        /// <summary>The running version matches the pending target — update confirmed, backup can be removed.</summary>
        Confirmed,
        /// <summary>The running version differs from the target and a backup exists — restore and relaunch the previous build.</summary>
        Rollback,
        /// <summary>The running version differs and no backup exists — drop the stale marker.</summary>
        ClearStaleMarker,
    }

    /// <summary>
    /// Pure, testable helpers for safe self-update: SHA-256 artifact verification
    /// and the startup rollback decision. No network, no filesystem writes here —
    /// the caller (AutoUpdateControl / Program) performs the actual I/O.
    /// </summary>
    public static class UpdateIntegrity
    {
        /// <summary>Computes the lowercase hex SHA-256 of a byte payload.</summary>
        public static string ComputeSha256(byte[] data)
        {
            return Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        }

        /// <summary>
        /// Verifies a downloaded artifact against its expected SHA-256. The
        /// expected value is trimmed and compared case-insensitively, tolerating
        /// surrounding whitespace or a trailing filename comment.
        /// </summary>
        public static bool VerifySha256(byte[] data, string expected)
        {
            if (data is null || data.Length == 0 || string.IsNullOrWhiteSpace(expected)) return false;

            string clean = expected.Trim();
            // A checksum line may look like "abc123...  Asus.zip" — take the first token.
            int space = clean.IndexOfAny(new[] { ' ', '\t' });
            if (space > 0) clean = clean[..space];

            return string.Equals(ComputeSha256(data), clean.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Decides what startup must do about a pending update.
        /// <paramref name="pendingVersion"/> is the version string stored before
        /// the swap (e.g. "1.4.0"); <paramref name="runningVersion"/> is the
        /// currently executing build; <paramref name="backupExists"/> tells whether
        /// the previous executable was preserved for rollback.
        /// </summary>
        public static PendingUpdateAction Decide(string? pendingVersion, string runningVersion, bool backupExists)
        {
            if (string.IsNullOrWhiteSpace(pendingVersion)) return PendingUpdateAction.None;
            if (string.IsNullOrWhiteSpace(runningVersion)) return PendingUpdateAction.ClearStaleMarker;

            string pending = pendingVersion.Trim().TrimStart('v', 'V');

            // Same version running that we intended to install → the swap worked.
            if (VersionEquals(pending, runningVersion))
                return backupExists ? PendingUpdateAction.Confirmed : PendingUpdateAction.Confirmed;

            // Different version running and we kept a backup → restore the old build.
            if (backupExists) return PendingUpdateAction.Rollback;

            // Different version, no backup → the marker is stale, drop it.
            return PendingUpdateAction.ClearStaleMarker;
        }

        static bool VersionEquals(string a, string b)
        {
            // Tolerate "v" prefixes and compare as versions when both parse.
            // Missing components (e.g. "0.274" vs "0.274.0.0") are treated as 0,
            // because .NET's Version equality treats them as -1 and would flag a
            // successful update as a mismatch, falsely triggering rollback.
            if (Version.TryParse(a, out var va) && Version.TryParse(b, out var vb))
            {
                if (va.Major != vb.Major || va.Minor != vb.Minor) return false;
                int ba = va.Build < 0 ? 0 : va.Build;
                int bb = vb.Build < 0 ? 0 : vb.Build;
                if (ba != bb) return false;
                int ra = va.Revision < 0 ? 0 : va.Revision;
                int rb = vb.Revision < 0 ? 0 : vb.Revision;
                return ra == rb;
            }
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
