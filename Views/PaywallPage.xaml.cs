using ScanSale.ViewModels;

namespace ScanSale.Views;

public partial class PaywallPage : ContentPage
{
    private readonly PaywallViewModel _vm;

    public PaywallPage(PaywallViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearingAsync();
    }
}
