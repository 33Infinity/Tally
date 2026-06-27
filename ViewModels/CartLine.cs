using CommunityToolkit.Mvvm.ComponentModel;
using ScanSale.Models;

namespace ScanSale.ViewModels;

/// <summary>A line in the in-progress sale cart. Snapshots the item so the sale is
/// unaffected by later edits; quantity is mutable and drives the line totals.</summary>
public partial class CartLine : ObservableObject
{
    public CartLine(Item item, int quantity = 1)
    {
        ItemId = item.Id;
        Code = item.Code;
        Description = item.Description;
        QrCode = item.QrCode;
        UnitPrice = item.Price;
        TaxRate = item.TaxRate;
        Quantity = quantity;
    }

    public int ItemId { get; }
    public string Code { get; }
    public string Description { get; }
    public string QrCode { get; }
    public decimal UnitPrice { get; }
    public decimal TaxRate { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineSubtotal))]
    [NotifyPropertyChangedFor(nameof(LineTax))]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int _quantity;

    public decimal LineSubtotal => UnitPrice * Quantity;
    public decimal LineTax => Math.Round(UnitPrice * TaxRate / 100m, 2) * Quantity;
    public decimal LineTotal => LineSubtotal + LineTax;
}
