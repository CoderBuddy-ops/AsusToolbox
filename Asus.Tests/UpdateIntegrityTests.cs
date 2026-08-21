using System.Security.Cryptography;
using System.Text;
using Asus.AutoUpdate;
using Xunit;

namespace Asus.Tests;

/// <summary>
/// Self-update integrity (UpdateIntegrity): SHA-256 artifact verification and
/// the startup rollback decision (confirm / restore / clear stale marker).
/// </summary>
public class UpdateIntegrityTests
{
    [Fact]
    public void ComputeSha256_MatchesKnownVector()
    {
        // "abc" -> well-known SHA-256.
        byte[] data = Encoding.UTF8.GetBytes("abc");
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", UpdateIntegrity.ComputeSha256(data));
    }

    [Fact]
    public void VerifySha256_AcceptsMatchingChecksum()
    {
        byte[] data = Encoding.UTF8.GetBytes("hello");
        string expected = UpdateIntegrity.ComputeSha256(data);

        Assert.True(UpdateIntegrity.VerifySha256(data, expected));
        Assert.True(UpdateIntegrity.VerifySha256(data, expected.ToUpperInvariant()));
        Assert.True(UpdateIntegrity.VerifySha256(data, expected + "  Asus.zip")); // trailing filename comment
    }

    [Fact]
    public void VerifySha256_RejectsTamperedPayload()
    {
        byte[] data = Encoding.UTF8.GetBytes("hello");
        byte[] tampered = Encoding.UTF8.GetBytes("hell0");
        string expected = UpdateIntegrity.ComputeSha256(data);

        Assert.False(UpdateIntegrity.VerifySha256(tampered, expected));
    }

    [Fact]
    public void VerifySha256_RejectsEmptyOrMissing()
    {
        Assert.False(UpdateIntegrity.VerifySha256(Array.Empty<byte>(), "abc"));
        Assert.False(UpdateIntegrity.VerifySha256(null!, "abc"));
        Assert.False(UpdateIntegrity.VerifySha256(new byte[] { 1 }, ""));
        Assert.False(UpdateIntegrity.VerifySha256(new byte[] { 1 }, "   "));
    }

    [Fact]
    public void Decide_NoPendingUpdate_IsNone()
    {
        Assert.Equal(PendingUpdateAction.None, UpdateIntegrity.Decide(null, "1.4.0", backupExists: true));
        Assert.Equal(PendingUpdateAction.None, UpdateIntegrity.Decide("", "1.4.0", backupExists: true));
    }

    [Fact]
    public void Decide_RunningMatchesPending_IsConfirmed()
    {
        Assert.Equal(PendingUpdateAction.Confirmed, UpdateIntegrity.Decide("1.4.0", "1.4.0.0", backupExists: true));
        Assert.Equal(PendingUpdateAction.Confirmed, UpdateIntegrity.Decide("v1.4.0", "1.4.0.0", backupExists: false));
    }

    [Fact]
    public void Decide_RunningDiffers_WithBackup_IsRollback()
    {
        Assert.Equal(PendingUpdateAction.Rollback, UpdateIntegrity.Decide("1.5.0", "1.4.0.0", backupExists: true));
        Assert.Equal(PendingUpdateAction.Rollback, UpdateIntegrity.Decide("1.5.0", "1.4.0.0", backupExists: true));
    }

    [Fact]
    public void Decide_RunningDiffers_NoBackup_IsClearStaleMarker()
    {
        Assert.Equal(PendingUpdateAction.ClearStaleMarker, UpdateIntegrity.Decide("1.5.0", "1.4.0.0", backupExists: false));
    }

    [Fact]
    public void Decide_VersionComparisons_AreTolerant()
    {
        // Same release, different version-string lengths still match.
        Assert.Equal(PendingUpdateAction.Confirmed, UpdateIntegrity.Decide("0.274", "0.274.0.0", backupExists: true));
        Assert.Equal(PendingUpdateAction.Confirmed, UpdateIntegrity.Decide("0.274.0", "0.274.0.0", backupExists: true));
    }

    [Fact]
    public void Flush_PersistsPendingMarkerBeforeProcessExit()
    {
        // Regression: the self-updater sets "update_pending" then starts the swap
        // and exits immediately. The debounced config write must not lose the
        // marker, or the startup confirm/rollback safety net silently never runs.
        string dir = Path.Combine(Path.GetTempPath(), "Asus.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string cfg = Path.Combine(dir, "config.json");
        AppConfig.UseConfigFile(cfg);

        AppConfig.Set("update_pending", "0.275");
        AppConfig.Flush();

        Assert.True(File.Exists(cfg), "Flush() must write the config file synchronously");
        string onDisk = File.ReadAllText(cfg);
        Assert.Contains("update_pending", onDisk);
        Assert.Contains("0.275", onDisk);
    }
}
