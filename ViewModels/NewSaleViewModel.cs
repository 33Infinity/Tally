using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using ScanSale.Models;
using ScanSale.Services;

namespace ScanSale.ViewModels;

public partial class NewSaleViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SessionService _session;

    public NewSaleViewModel(DatabaseService db, SessionService session)
    {
        _db = db;
        _session = session;
        Cart.CollectionChanged += (_, _) => Recalculate();
    }

    public ObservableCollection<CartLine> Cart { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScanButtonLabel))]
    private bool _isScanning;
    [ObservableProperty] private string? _status;

    // Tap "Add item" → scan one → auto-stops. Button reflects that state.
    public string ScanButtonLabel => IsScanning ? "Cancel scan" : "Add item (scan)";

    [ObservableProperty] private int _itemCount;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _taxTotal;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _isEmpty = true;

    [RelayCommand]
    private async Task ToggleScanAsync()
    {
        if (IsScanning) { IsScanning = false; return; }
        if (!await EnsureCameraPermissionAsync()) return;
        IsScanning = true;
    }

    private async Task<bool> EnsureCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            Status = "Camera permission is required to scan. Enable it in Settings, or type the code instead.";
            return false;
        }
        return true;
    }

    /// <summary>Called from the camera scanner. One scan per "Add item" tap: stop
    /// immediately so the camera tears down and queued repeat detections are ignored.</summary>
    public async Task OnScannedAsync(string value)
    {
        if (!IsScanning) return;            // already handled this tap's scan
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) return;
        IsScanning = false;                 // auto-stop after a single read
        await AddByCodeAsync(value);
    }

    private async Task AddByCodeAsync(string code)
    {
        var item = await _db.GetItemByQrAsync(_session.CurrentUserId, code);
        if (item is null)
        {
            Status = $"No item matches QR “{code}”.";
            return;
        }

        var existing = Cart.FirstOrDefault(l => l.ItemId == item.Id);
        if (existing is not null)
        {
            existing.Quantity++;
            var idx = Cart.IndexOf(existing);
            if (idx > 0) Cart.Move(idx, 0); // bring the just-scanned item to the top
            Recalculate();
        }
        else
        {
            Cart.Insert(0, new CartLine(item)); // newest first; CollectionChanged → Recalculate
        }
        // Live tally — visible even while the camera preview crowds the cart list.
        Status = $"Added {item.Description} — {ItemCount} item(s), {Total:C} so far.";
    }

    [RelayCommand]
    private void Increment(CartLine line)
    {
        if (line is null) return;
        line.Quantity++;
        Recalculate();
    }

    [RelayCommand]
    private void Decrement(CartLine line)
    {
        if (line is null) return;
        if (line.Quantity <= 1) Cart.Remove(line);
        else { line.Quantity--; Recalculate(); }
    }

    [RelayCommand]
    private void Remove(CartLine line)
    {
        if (line is not null) Cart.Remove(line);
    }

    [RelayCommand]
    private void Clear()
    {
        Cart.Clear();
        Status = null;
    }

    [RelayCommand]
    private async Task CompleteAsync()
    {
        if (Cart.Count == 0) { Status = "Scan or add at least one item first."; return; }

        var lines = Cart.Select(l => new SaleLine
        {
            ItemId = l.ItemId,
            Code = l.Code,
            Description = l.Description,
            QrCode = l.QrCode,
            UnitPrice = l.UnitPrice,
            TaxRate = l.TaxRate,
            Quantity = l.Quantity,
            LineSubtotal = l.LineSubtotal,
            LineTax = l.LineTax,
            LineTotal = l.LineTotal
        }).ToList();

        var sale = new Sale
        {
            UserId = _session.CurrentUserId,
            CreatedUtc = DateTime.UtcNow,
            ItemCount = lines.Sum(l => l.Quantity),
            Subtotal = lines.Sum(l => l.LineSubtotal),
            TaxTotal = lines.Sum(l => l.LineTax),
            Total = lines.Sum(l => l.LineTotal)
        };

        await _db.SaveSaleAsync(sale, lines);
        var total = sale.Total;
        Cart.Clear();
        IsScanning = false;
        Status = null;
        await Shell.Current.DisplayAlert("Sale complete", $"Recorded {sale.ItemCount} item(s), total {total:C}.", "OK");
    }

    private void Recalculate()
    {
        ItemCount = Cart.Sum(l => l.Quantity);
        Subtotal = Cart.Sum(l => l.LineSubtotal);
        TaxTotal = Cart.Sum(l => l.LineTax);
        Total = Cart.Sum(l => l.LineTotal);
        IsEmpty = Cart.Count == 0;
    }
}
