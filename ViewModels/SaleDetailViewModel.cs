using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ScanSale.Models;
using ScanSale.Services;

namespace ScanSale.ViewModels;

[QueryProperty(nameof(SaleId), "id")]
public partial class SaleDetailViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    public SaleDetailViewModel(DatabaseService db) => _db = db;

    public ObservableCollection<SaleLine> Lines { get; } = new();

    [ObservableProperty] private int _saleId;
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private int _itemCount;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _taxTotal;
    [ObservableProperty] private decimal _total;

    partial void OnSaleIdChanged(int value) => _ = LoadAsync(value);

    private async Task LoadAsync(int id)
    {
        if (id <= 0) return;
        var sale = await _db.GetSaleAsync(id);
        if (sale is not null) Date = sale.CreatedLocal;

        var lines = await _db.GetSaleLinesAsync(id);
        Lines.Clear();
        foreach (var l in lines) Lines.Add(l);

        ItemCount = lines.Sum(l => l.Quantity);
        Subtotal = lines.Sum(l => l.LineSubtotal);
        TaxTotal = lines.Sum(l => l.LineTax);
        Total = lines.Sum(l => l.LineTotal);
    }
}
