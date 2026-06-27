using SQLite;

namespace ScanSale.Models;

/// <summary>
/// A sellable item the user has entered. Scoped to the owning <see cref="AppUser"/>.
/// The <see cref="QrCode"/> is the value matched when scanning during a sale.
/// </summary>
[Table("items")]
public class Item
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Owning user (offline scoping).</summary>
    [Indexed]
    public int UserId { get; set; }

    /// <summary>User-assigned item id / SKU.</summary>
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Unit price before tax.</summary>
    public decimal Price { get; set; }

    /// <summary>Tax rate as a percentage (e.g. 8.25 = 8.25%).</summary>
    public decimal TaxRate { get; set; }

    /// <summary>The QR payload that identifies this item when scanned. Indexed for fast lookup.</summary>
    [Indexed]
    public string QrCode { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // ── Convenience (not persisted) ──────────────────────────────────────────
    [Ignore] public decimal TaxAmount => Math.Round(Price * TaxRate / 100m, 2);
    [Ignore] public decimal PriceWithTax => Price + TaxAmount;
    [Ignore] public string Display => $"{Code} — {Description}";
}
