using System.Windows;

namespace RemoteCompute.Client;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _running;

    public MainWindow() => InitializeComponent();

    private async void Gaming_Click(object sender, RoutedEventArgs e) => await LaunchAsync(true);
    private async void Coding_Click(object sender, RoutedEventArgs e) => await LaunchAsync(false);

    private async Task LaunchAsync(bool gaming)
    {
        if (_running is not null) return;
        _running = new CancellationTokenSource();
        GamingButton.IsEnabled = CodeButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        try
        {
            await new LaunchCoordinator(AppSettings.Load()).StartAsync(gaming,
                status => Dispatcher.Invoke(() => StatusText.Text = status), _running.Token);
        }
        catch (OperationCanceledException) { StatusText.Text = "Cancelled"; }
        catch (Exception ex) { StatusText.Text = $"Error: {ex.Message}"; }
        finally
        {
            _running.Dispose();
            _running = null;
            GamingButton.IsEnabled = CodeButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => _running?.Cancel();
}
