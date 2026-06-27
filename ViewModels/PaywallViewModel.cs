using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScanSale.Services;

namespace ScanSale.ViewModels;

public partial class PaywallViewModel : ObservableObject
{
    private readonly SubscriptionService _subs;
    private readonly SessionService _session;

    public PaywallViewModel(SubscriptionService subs, SessionService session)
    {
        _subs = subs;
        _session = session;
    }

    [ObservableProperty] private bool _busy;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private string _priceLine = "7-day free trial, then monthly";

    public async Task OnAppearingAsync()
    {
        var price = await _subs.GetPriceAsync();
        if (!string.IsNullOrWhiteSpace(price))
            PriceLine = $"7-day free trial, then {price}/month";
    }

    [RelayCommand]
    private async Task StartTrialAsync()
    {
        if (Busy) return;
        Busy = true; Error = null;
        try
        {
            if (await _subs.PurchaseAsync())
                await Shell.Current.GoToAsync("//main");
            else
                Error = "Couldn’t start the subscription. Please try again.";
        }
        catch (Exception ex)
        {
            Error = "Subscription error: " + ex.Message;
        }
        finally { Busy = false; }
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        if (Busy) return;
        Busy = true; Error = null;
        try
        {
            if (await _subs.RefreshAsync())
                await Shell.Current.GoToAsync("//main");
            else
                Error = "No active subscription found on this Google account.";
        }
        finally { Busy = false; }
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        _session.SignOut();
        await Shell.Current.GoToAsync("//login");
    }
}
