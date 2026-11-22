using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace RemoteCompute.Client;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        try
        {
            var settings = AppSettings.Load();
            var target = settings.RelaySshTarget.Split('@', 2);
            if (target.Length == 2)
            {
                if (target[0] != "YOUR_LENOVO_USER")
                    RelayUser.Text = target[0];
                if (target[1] != "100.x.x.x")
                    RelayIp.Text = target[1];
            }

            if (settings.DesktopTailscaleIp != "100.y.y.y")
                DesktopIp.Text = settings.DesktopTailscaleIp;
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Couldn't load settings: {ex.Message}";
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var user = RelayUser.Text.Trim();
        var relayIp = RelayIp.Text.Trim();
        var desktopIp = DesktopIp.Text.Trim();

        if (user.Length == 0 || user.StartsWith('-') || user.Contains('@') || user.Any(char.IsWhiteSpace))
        {
            ErrorText.Text = "Enter the Lenovo SSH username.";
            RelayUser.Focus();
            return;
        }

        if (!IsIpv4(relayIp))
        {
            ErrorText.Text = "Enter the Lenovo’s Tailscale IPv4 address.";
            RelayIp.Focus();
            return;
        }

        if (!IsIpv4(desktopIp))
        {
            ErrorText.Text = "Enter the gaming desktop’s Tailscale IPv4 address.";
            DesktopIp.Focus();
            return;
        }

        try
        {
            AppSettings.SaveConnection(new AppSettings.ConnectionSettings($"{user}@{relayIp}", desktopIp));
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Couldn't save settings: {ex.Message}";
        }
    }

    private static bool IsIpv4(string value) =>
        IPAddress.TryParse(value, out var ip) &&
        ip.AddressFamily == AddressFamily.InterNetwork &&
        ip.ToString() == value;
}
