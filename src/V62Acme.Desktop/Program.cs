using System.Text.Json;
using Velopack;
using Velopack.Sources;

VelopackApp.Build().Run();
var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "f62-app.log");
void Log(string m) => File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} {m}\n");
var cfg = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));
var src = cfg.RootElement.GetProperty("Updates").GetProperty("Source").GetString()!;
try
{
    var mgr = src.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase) ? new UpdateManager(new GithubSource(src, null, false)) : new UpdateManager(src);
    Log($"start, version {mgr.CurrentVersion?.ToString() ?? "(not installed)"}, source {src}");
    if (!mgr.IsInstalled) { Log("not installed: check off"); return; }
    var update = await mgr.CheckForUpdatesAsync();
    if (update == null) { Log("no update"); return; }
    Log($"update {update.TargetFullRelease.Version} found, {update.DeltasToTarget.Length} delta(s)");
    await mgr.DownloadUpdatesAsync(update);
    Log("downloaded; applying and restarting");
    mgr.ApplyUpdatesAndRestart(update);
}
catch (Exception e) { Log("update check failed, the app keeps its version: " + e.Message); }
