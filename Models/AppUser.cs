using SQLite;

namespace ScanSale.Models;

/// <summary>A local, offline user profile. No server — credentials live on-device.</summary>
[Table("users")]
public class AppUser
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Unique login name (case-insensitive match enforced in code).</summary>
    [Indexed(Unique = true)]
    public string Username { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of (username + PIN). The raw PIN is never stored.</summary>
    public string PinHash { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
