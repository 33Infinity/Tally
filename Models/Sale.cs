using SQLite;

namespace ScanSale.Models;

/// <summary>A completed sale (header). Line items are <see cref="SaleLine"/> rows.</summary>
[Table("sales")]
public class Sale
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }

    [Ignore] public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
}
