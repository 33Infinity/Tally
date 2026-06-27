using ScanSale.Views;

namespace ScanSale;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Detail routes pushed on top of the tabs.
        Routing.RegisterRoute("itemedit", typeof(ItemEditPage));
        Routing.RegisterRoute("saledetail", typeof(SaleDetailPage));
    }
}
