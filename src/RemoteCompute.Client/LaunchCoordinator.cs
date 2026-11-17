using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace RemoteCompute.Client;

public sealed class LaunchCoordinator(AppSettings settings)
{
    public async Task StartAsync(bool gaming, Action<string> report, CancellationToken token)
    {
        report("Requesting wake through the Lenovo relay…");
        // SSH key authentication is required: batch mode prevents invisible password prompts.
        // Relay is configured to only allow the fixed WoL command for this SSH key.
        await RunAsync("ssh", ["-o", "BatchMode=yes", "-o", "ConnectTimeout=10",
            settings.RelaySshTarget, "wake-desktop"], token);

        report("Waiting for the desktop to become reachable…");
        var deadline = DateTimeOffset.UtcNow.AddSeconds(settings.WakeTimeoutSeconds);
        while (DateTimeOffset.UtcNow < deadline)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                using var tcp = new TcpClient();
                using var attempt = CancellationTokenSource.CreateLinkedTokenSource(token);
                attempt.CancelAfter(TimeSpan.FromSeconds(2));
                await tcp.ConnectAsync(settings.DesktopTailscaleIp, settings.DesktopReadyPort, attempt.Token);
                report("Desktop reachable. Opening application…");
                if (gaming)
                    StartDetached(settings.MoonlightPath);
                else
                    StartDetached(settings.VscodePath, ["--remote", $"ssh-remote+{settings.VscodeSshHost}"]);
                report("Launched. Automatic sleep is not enabled in this starter release.");
                return;
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested) { }
            catch (SocketException) { }
            await Task.Delay(2000, token);
        }
        throw new TimeoutException("Desktop didn't respond. Check WoL, Tailscale and the configured TCP readiness port.");
    }

    private static void StartDetached(string exe, IEnumerable<string>? args = null)
    {
        if (!File.Exists(exe)) throw new FileNotFoundException("Application not found", exe);
        var info = new ProcessStartInfo(exe) { UseShellExecute = false };
        if (args is not null) foreach (var arg in args) info.ArgumentList.Add(arg);
        _ = Process.Start(info) ?? throw new InvalidOperationException($"Could not start {exe}");
    }

    private static async Task RunAsync(string exe, IEnumerable<string> args, CancellationToken token)
    {
        using var process = new Process { StartInfo = new ProcessStartInfo(exe)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardError = true, RedirectStandardOutput = true
        }};
        foreach (var arg in args) process.StartInfo.ArgumentList.Add(arg);
        if (!process.Start()) throw new InvalidOperationException($"Could not start {exe}");
        var stderrTask = process.StandardError.ReadToEndAsync(token);
        var stdoutTask = process.StandardOutput.ReadToEndAsync(token);
        try { await process.WaitForExitAsync(token); }
        catch (OperationCanceledException) { if (!process.HasExited) process.Kill(entireProcessTree: true); throw; }
        var err = await stderrTask;
        var output = await stdoutTask;
        if (process.ExitCode != 0) throw new InvalidOperationException($"Remote wake failed: {err} {output}".Trim());
    }
}
