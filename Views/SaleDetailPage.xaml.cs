using ScanSale.ViewModels;

namespace ScanSale.Views;

public partial class SaleDetailPage : ContentPage
{
    public SaleDetailPage(SaleDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
