using SQLite;

namespace ScanSale.Models;

/// <summary>One line of a <see cref="Sale"/>. Item details are snapshotted so history
/// stays accurate even if the underlying item is later edited or deleted.</summary>
[Table("sale_lines")]
public class SaleLine
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SaleId { get; set; }

    /// <summary>Source item id (may no longer exist; details below are the snapshot).</summary>
    public int ItemId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int Quantity { get; set; }

    public decimal LineSubtotal { get; set; }
    public decimal LineTax { get; set; }
    public decimal LineTotal { get; set; }
}
