using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScanSale.Services;

namespace ScanSale.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly SubscriptionService _subs;
    public LoginViewModel(SessionService session, SubscriptionService subs)
    {
        _session = session;
        _subs = subs;
    }

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _busy;

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (Busy) return;
        Busy = true;
        Error = null;
        try
        {
            var err = await _session.SignInOrCreateAsync(Username, Pin);
            if (err is not null) { Error = err; return; }
            Pin = string.Empty;
            // Gate on subscription: trial-active or paid → app; otherwise paywall.
            var entitled = await _subs.RefreshAsync();
            await Shell.Current.GoToAsync(entitled ? "//main" : "//paywall");
        }
        catch (Exception ex)
        {
            Error = "Could not sign in: " + ex.Message;
        }
        finally { Busy = false; }
    }
}
