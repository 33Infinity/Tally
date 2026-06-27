using ScanSale.ViewModels;

namespace ScanSale.Views;

public partial class ItemsPage : ContentPage
{
    private readonly ItemsViewModel _vm;

    public ItemsPage(ItemsViewModel vm)
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
