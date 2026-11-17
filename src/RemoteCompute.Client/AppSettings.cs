using System.IO;
using System.Text.Json;

namespace RemoteCompute.Client;

public sealed record AppSettings(
    string RelaySshTarget,
    string DesktopTailscaleIp,
    int DesktopReadyPort,
    int WakeTimeoutSeconds,
    string MoonlightPath,
    string VscodePath,
    string VscodeSshHost)
{
    public static AppSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("appsettings.json is empty");

        if (string.IsNullOrWhiteSpace(settings.RelaySshTarget) ||
            string.IsNullOrWhiteSpace(settings.DesktopTailscaleIp) ||
            settings.DesktopReadyPort is < 1 or > 65535 ||
            settings.WakeTimeoutSeconds is < 5 or > 600)
            throw new InvalidDataException("Check relay, desktop IP, readiness port and timeout in appsettings.json");
        return settings;
    }
}
