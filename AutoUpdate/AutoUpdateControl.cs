using Asus.Helpers;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Asus.AutoUpdate
{
    public class AutoUpdateControl
    {

        SettingsForm settings;

        // ── Repo identity (single source of truth) ──────────────────────────────
        // Fork owners: set these to YOUR GitHub repository, e.g.
        //   RepoOwner = "your-username";
        //   RepoName  = "your-repo-name";
        // The release endpoints derive from these two values. The API URL can be
        // overridden at runtime via the "update_api_url" config key — used by
        // forks to point at their own repository without recompiling, and by
        // tests to exercise the updater against a local release server.
        public const string RepoOwner = "seerge";
        public const string RepoName = "g-helper";

        public string versionUrl => $"https://github.com/{RepoOwner}/{RepoName}/releases";
        public string apiUrl => AppConfig.GetString("update_api_url", $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");

        /// <summary>
        /// Starts a detached PowerShell that performs file operations after this
        /// process exits (Wait-Process blocks until the app is gone, since a
        /// running exe cannot be replaced in place).
        /// </summary>
        static void RunDetachedPowerShell(string workingDir, string command)
        {
            var cmd = new Process();
            cmd.StartInfo.WorkingDirectory = workingDir;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.FileName = "powershell";
            cmd.StartInfo.Arguments = command;
            if (ProcessHelper.IsUserAdministrator()) cmd.StartInfo.Verb = "runas";
            cmd.Start();
        }

        public bool update = false;

        static long lastUpdate;

        public AutoUpdateControl(SettingsForm settingsForm)
        {
            settings = settingsForm;
            var appVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            settings.SetVersionLabel(Properties.Strings.VersionLabel + $": {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}");
        }

        public void CheckForUpdates()
        {
            // Run update once per 12 hours
            if (Math.Abs(DateTimeOffset.Now.ToUnixTimeSeconds() - lastUpdate) < 43200) return;
            lastUpdate = DateTimeOffset.Now.ToUnixTimeSeconds();

            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                CheckForUpdatesAsync();
            });
        }

        public void Update()
        {
            if (update)
            {
                Task.Run(() =>
                {
                    CheckForUpdatesAsync(true);
                });
            } else
            {
                LoadReleases();
            }
        }

        public void LoadReleases()
        {
            try
            {
                Process.Start(new ProcessStartInfo(versionUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Failed to open releases page:" + ex.Message);
            }
        }

        async void CheckForUpdatesAsync(bool force = false)
        {

            if (AppConfig.Is("skip_updates")) return;

            try
            {

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Asus App");
                    var json = await httpClient.GetStringAsync(apiUrl);
                    var config = JsonSerializer.Deserialize<JsonElement>(json);
                    var tag = config.GetProperty("tag_name").ToString().Replace("v", "");
                    var assets = config.GetProperty("assets");

                    string zipUrl = null;
                    string shaUrl = null;

                    for (int i = 0; i < assets.GetArrayLength(); i++)
                    {
                        string assetName = assets[i].GetProperty("name").ToString();
                        string downloadUrl = assets[i].GetProperty("browser_download_url").ToString();

                        if (assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && zipUrl is null)
                            zipUrl = downloadUrl;
                        else if (assetName.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) || assetName.EndsWith(".sha256.txt", StringComparison.OrdinalIgnoreCase))
                            shaUrl = downloadUrl;
                    }

                    if (zipUrl is null && assets.GetArrayLength() > 0)
                        zipUrl = assets[0].GetProperty("browser_download_url").ToString();

                    var gitVersion = new Version(tag);
                    var appVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    //appVersion = new Version("0.50.0.0"); 

                    if (gitVersion.CompareTo(appVersion) > 0)
                    {
                        update = true;
                        settings.SetVersionLabel(Properties.Strings.DownloadUpdate + $": {appVersion.Major}.{appVersion.Minor}.{appVersion.Build} → {tag}", true);

                        string[] args = Environment.GetCommandLineArgs();
                        if (force || args.Length > 1 && args[1] == "autoupdate")
                        {
                            AutoUpdate(zipUrl, shaUrl, tag);
                            return;
                        }

                        if (AppConfig.GetString("skip_version") != tag)
                        {
                            DialogResult dialogResult = DialogResult.No;

                            settings.Invoke((System.Windows.Forms.MethodInvoker)delegate
                            {
                                dialogResult = MessageBox.Show(settings, Properties.Strings.DownloadUpdate + ": Asus " + tag + "?", "Update", MessageBoxButtons.YesNo);
                            });
                            
                            if (dialogResult == DialogResult.Yes)
                                AutoUpdate(zipUrl, shaUrl, tag);
                            else
                                AppConfig.Set("skip_version", tag);
                        }

                    }
                    else
                    {
                        Logger.WriteLine($"Latest version {appVersion}");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Failed to check for updates:" + ex.Message);
            }

        }

        public static string EscapeString(string input)
        {
            return Regex.Replace(Regex.Replace(input, @"\[|\]", "`$0"), @"\'", "''");
        }

        async void AutoUpdate(string requestUri, string shaUri, string tag)
        {

            Uri uri = new Uri(requestUri);
            string zipName = Path.GetFileName(uri.LocalPath);

            string exeLocation = Application.ExecutablePath;
            string exeDir = Path.GetDirectoryName(exeLocation);
            //exeDir = "C:\\Program Files\\Asus";
            string exeName = Path.GetFileName(exeLocation);
            string zipLocation = exeDir + "\\" + zipName;

            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Add("User-Agent", "Asus App");
                Logger.WriteLine(requestUri);
                Logger.WriteLine(exeDir);
                Logger.WriteLine(zipName);
                Logger.WriteLine(exeName);

                byte[] bytes;

                try
                {
                    bytes = await client.GetByteArrayAsync(uri);
                    Logger.WriteLine($"Downloaded {bytes.Length}b");
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex.Message);
                    if (!ProcessHelper.IsUserAdministrator())
                    {
                        ProcessHelper.RunAsAdmin("autoupdate");
                        Application.Exit();
                    } else
                    {
                        LoadReleases();
                    }
                    return;
                }

                // ── SHA-256 verification (mandatory) ───────────────────────────
                if (string.IsNullOrWhiteSpace(shaUri))
                {
                    Logger.WriteLine("Update REFUSED: release has no .sha256 checksum asset.");
                    MessageBox.Show(settings, "This release has no integrity checksum. Update refused.", "Asus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    string expected = await client.GetStringAsync(shaUri);
                    if (!UpdateIntegrity.VerifySha256(bytes, expected))
                    {
                        Logger.WriteLine("Update REFUSED: SHA-256 mismatch.");
                        MessageBox.Show(settings, "Integrity check failed — the downloaded update does not match its checksum. Update aborted.", "Asus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Logger.WriteLine($"SHA-256 verified ({UpdateIntegrity.ComputeSha256(bytes)})");
                }
                catch (Exception ex)
                {
                    Logger.WriteLine("Checksum fetch failed: " + ex.Message);
                    MessageBox.Show(settings, "Could not fetch the release checksum. Update aborted.", "Asus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                File.WriteAllBytes(zipLocation, bytes);
                Logger.WriteLine($"Staged {bytes.Length}b: {zipLocation}");

                // Mark the pending update so the next launch can confirm or roll back.
                // Flush synchronously: the process exits right after starting the
                // swap, and the debounced write would otherwise be lost, silently
                // disabling the startup confirm/rollback safety net.
                AppConfig.Set("update_pending", tag);
                AppConfig.Flush();

                string backupName = exeName + ".bak";
                string command = $"$ErrorActionPreference = \"Stop\"; Set-Location -Path '{EscapeString(exeDir)}'; Wait-Process -Name \"Asus\"; Copy-Item \"{exeName}\" \"{backupName}\" -Force -ErrorAction SilentlyContinue; Expand-Archive \"{zipName}\" -DestinationPath . -Force; Remove-Item \"{zipName}\" -Force; \".\\{exeName}\"; ";
                Logger.WriteLine(command);

                try
                {
                    RunDetachedPowerShell(exeDir, command);
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex.Message);
                }

                Application.Exit();
            }

        }

        /// <summary>
        /// Startup check for a pending update: confirms the new build, or
        /// restores the backed-up previous build if the swap never took effect.
        /// Returns true when the process should relaunch (rollback performed).
        /// </summary>
        public static bool HandlePendingUpdate()
        {
            string pending = AppConfig.GetString("update_pending");
            if (string.IsNullOrWhiteSpace(pending)) return false;

            string running = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string exeLocation = Application.ExecutablePath;
            string exeDir = Path.GetDirectoryName(exeLocation);
            string exeName = Path.GetFileName(exeLocation);
            string backupPath = Path.Combine(exeDir, exeName + ".bak");

            var action = UpdateIntegrity.Decide(pending, running, File.Exists(backupPath));
            Logger.WriteLine($"Pending update '{pending}' vs running '{running}': {action}");

            switch (action)
            {
                case PendingUpdateAction.Confirmed:
                    // New build is live and matches the target — clean up.
                    AppConfig.Remove("update_pending");
                    try { if (File.Exists(backupPath)) File.Delete(backupPath); } catch (Exception ex) { Logger.WriteLine("Backup cleanup: " + ex.Message); }
                    return false;

                case PendingUpdateAction.Rollback:
                    try
                    {
                        // The running exe is not the target and we kept a backup.
                        // We cannot overwrite our own running image, so a detached
                        // helper waits for this process to exit, restores the
                        // backup over the exe, and relaunches the previous build.
                        string command = $"$ErrorActionPreference = \"Stop\"; Set-Location -Path '{EscapeString(exeDir)}'; Wait-Process -Name \"Asus\"; Copy-Item \"{exeName}.bak\" \"{exeName}\" -Force; \".\\{exeName}\"; ";
                        RunDetachedPowerShell(exeDir, command);

                        // Clear the marker now so the restored build starts clean;
                        // flush synchronously so it survives this process exiting.
                        AppConfig.Remove("update_pending");
                        AppConfig.Flush();
                        Logger.WriteLine("Update failed to apply — restored previous build.");
                        return true; // this process exits; the helper relaunches the old build
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine("Rollback failed: " + ex.Message);
                        AppConfig.Remove("update_pending");
                        AppConfig.Flush();
                        return false;
                    }

                case PendingUpdateAction.ClearStaleMarker:
                    AppConfig.Remove("update_pending");
                    return false;

                default:
                    return false;
            }
        }

    }
}
