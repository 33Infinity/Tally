using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using ScanSale.Models;
using ScanSale.Services;

namespace ScanSale.ViewModels;

[QueryProperty(nameof(ItemId), "id")]
public partial class ItemEditViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SessionService _session;
    private Item _item = new();

    public ItemEditViewModel(DatabaseService db, SessionService session)
    {
        _db = db;
        _session = session;
    }

    // Set via navigation query (?id=). 0 = new item.
    [ObservableProperty] private int _itemId;

    [ObservableProperty] private string _title = "New Item";

    /// <summary>The scanned QR/barcode value. It IS the item id and the scan-match key.</summary>
    [ObservableProperty] private string _code = string.Empty;

    [ObservableProperty] private string _description = string.Empty;   // optional
    [ObservableProperty] private string _priceText = string.Empty;
    [ObservableProperty] private string _taxText = string.Empty;

    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private bool _canDelete;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private string? _status;

    partial void OnItemIdChanged(int value) => _ = LoadAsync(value);

    private async Task LoadAsync(int id)
    {
        if (id <= 0) { Title = "New Item"; CanDelete = false; return; }
        var existing = await _db.GetItemAsync(id);
        if (existing is null) return;
        _item = existing;
        Title = "Edit Item";
        // Code and QrCode are the same scanned value; prefer whichever is populated.
        Code = string.IsNullOrWhiteSpace(existing.QrCode) ? existing.Code : existing.QrCode;
        Description = existing.Description;
        PriceText = existing.Price.ToString(CultureInfo.CurrentCulture);
        TaxText = existing.TaxRate.ToString(CultureInfo.CurrentCulture);
        CanDelete = true;
    }

    [RelayCommand]
    private async Task ToggleScanAsync()
    {
        if (IsScanning) { IsScanning = false; return; }
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            Status = "Camera permission is required to scan. Enable it in Settings, or type the id instead.";
            return;
        }
        IsScanning = true;
    }

    /// <summary>Called by the camera scanner — the scanned value auto-fills the item id.</summary>
    public Task OnScannedAsync(string value)
    {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) return Task.CompletedTask;
        Code = value;
        IsScanning = false;
        Status = $"Scanned item id: {value}";
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        Error = null;
        if (string.IsNullOrWhiteSpace(Code))
        { Error = "Scan the item's QR/barcode to set its id (or type it)."; return; }
        if (!decimal.TryParse(PriceText, NumberStyles.Number, CultureInfo.CurrentCulture, out var price) || price < 0)
        { Error = "Enter a valid price."; return; }
        var tax = 0m;
        if (!string.IsNullOrWhiteSpace(TaxText) &&
            (!decimal.TryParse(TaxText, NumberStyles.Number, CultureInfo.CurrentCulture, out tax) || tax < 0))
        { Error = "Enter a valid tax rate (or leave blank)."; return; }

        var value = Code.Trim();
        _item.UserId = _session.CurrentUserId;
        _item.Code = value;       // item id
        _item.QrCode = value;     // same value is matched when scanning during a sale
        _item.Description = (Description ?? string.Empty).Trim();
        _item.Price = price;
        _item.TaxRate = tax;

        await _db.SaveItemAsync(_item);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_item.Id == 0) return;
        var label = string.IsNullOrWhiteSpace(_item.Description) ? _item.Code : _item.Description;
        var confirm = await Shell.Current.DisplayAlert("Delete item",
            $"Delete \"{label}\"? This does not affect past sales.", "Delete", "Cancel");
        if (!confirm) return;
        await _db.DeleteItemAsync(_item);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");
}
