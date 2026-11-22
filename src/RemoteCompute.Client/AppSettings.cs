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
    private static string ConnectionPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RemoteCompute", "connection.json");

    internal sealed record ConnectionSettings(string RelaySshTarget, string DesktopTailscaleIp);

    internal static void SaveConnection(ConnectionSettings connection)
    {
        var path = ConnectionPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(connection));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    public static AppSettings Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("appsettings.json is empty");

        if (File.Exists(ConnectionPath))
        {
            var connection = JsonSerializer.Deserialize<ConnectionSettings>(File.ReadAllText(ConnectionPath))
                ?? throw new InvalidDataException("Connection settings are empty.");
            settings = settings with
            {
                RelaySshTarget = connection.RelaySshTarget,
                DesktopTailscaleIp = connection.DesktopTailscaleIp
            };
        }

        if (string.IsNullOrWhiteSpace(settings.RelaySshTarget) ||
            string.IsNullOrWhiteSpace(settings.DesktopTailscaleIp) ||
            settings.DesktopReadyPort is < 1 or > 65535 ||
            settings.WakeTimeoutSeconds is < 5 or > 600)
            throw new InvalidDataException("Check relay, desktop IP, readiness port and timeout in appsettings.json");
        return settings;
    }
}
