using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScanSale.Models;
using ScanSale.Services;

namespace ScanSale.ViewModels;

public partial class SalesHistoryViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SessionService _session;

    public SalesHistoryViewModel(DatabaseService db, SessionService session)
    {
        _db = db;
        _session = session;
    }

    public ObservableCollection<Sale> Sales { get; } = new();

    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private decimal _grandTotal;

    public async Task LoadAsync()
    {
        if (!_session.IsLoggedIn) return;
        var list = await _db.GetSalesAsync(_session.CurrentUserId);
        Sales.Clear();
        foreach (var s in list) Sales.Add(s);
        IsEmpty = Sales.Count == 0;
        GrandTotal = list.Sum(s => s.Total);
    }

    [RelayCommand]
    private Task OpenAsync(Sale sale) =>
        sale is null ? Task.CompletedTask : Shell.Current.GoToAsync($"saledetail?id={sale.Id}");
}
