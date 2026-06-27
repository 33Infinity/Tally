using ScanSale.ViewModels;

namespace ScanSale.Views;

public partial class SalesHistoryPage : ContentPage
{
    private readonly SalesHistoryViewModel _vm;

    public SalesHistoryPage(SalesHistoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
