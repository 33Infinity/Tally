using ScanSale.Models;
using SQLite;

namespace ScanSale.Services;

/// <summary>
/// All on-device persistence (offline-first) via sqlite-net. The database file
/// lives under <see cref="FileSystem.AppDataDirectory"/> — local to the device,
/// no network. A single async connection is shared across the app.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _conn;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "scansale.db3");

    private async Task<SQLiteAsyncConnection> ConnAsync()
    {
        if (_conn is not null) return _conn;
        _conn = new SQLiteAsyncConnection(
            DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _conn.CreateTableAsync<AppUser>();
        await _conn.CreateTableAsync<Item>();
        await _conn.CreateTableAsync<Sale>();
        await _conn.CreateTableAsync<SaleLine>();
        return _conn;
    }

    // ── Users ────────────────────────────────────────────────────────────────
    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        var c = await ConnAsync();
        var u = (username ?? string.Empty).Trim().ToLowerInvariant();
        return await c.Table<AppUser>().Where(x => x.Username == u).FirstOrDefaultAsync();
    }

    public async Task<AppUser> CreateUserAsync(string username, string pinHash)
    {
        var c = await ConnAsync();
        var user = new AppUser
        {
            Username = (username ?? string.Empty).Trim().ToLowerInvariant(),
            PinHash = pinHash
        };
        await c.InsertAsync(user);
        return user;
    }

    // ── Items ────────────────────────────────────────────────────────────────
    public async Task<List<Item>> GetItemsAsync(int userId)
    {
        var c = await ConnAsync();
        return await c.Table<Item>().Where(x => x.UserId == userId)
            .OrderBy(x => x.Description).ToListAsync();
    }

    public async Task<Item?> GetItemAsync(int id)
    {
        var c = await ConnAsync();
        return await c.Table<Item>().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>Finds a user's item by its QR payload (case-insensitive, trimmed).</summary>
    public async Task<Item?> GetItemByQrAsync(int userId, string qr)
    {
        var c = await ConnAsync();
        var needle = (qr ?? string.Empty).Trim();
        var matches = await c.Table<Item>().Where(x => x.UserId == userId && x.QrCode == needle).ToListAsync();
        if (matches.Count > 0) return matches[0];
        // Fallback: case-insensitive scan (sqlite-net's LINQ '==' is case-sensitive).
        var all = await c.Table<Item>().Where(x => x.UserId == userId).ToListAsync();
        return all.FirstOrDefault(x => string.Equals(x.QrCode?.Trim(), needle, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<int> SaveItemAsync(Item item)
    {
        var c = await ConnAsync();
        return item.Id == 0 ? await c.InsertAsync(item) : await c.UpdateAsync(item);
    }

    public async Task<int> DeleteItemAsync(Item item)
    {
        var c = await ConnAsync();
        return await c.DeleteAsync(item);
    }

    // ── Sales ──────────────────────────────────────────────────────────────────
    public async Task<int> SaveSaleAsync(Sale sale, IEnumerable<SaleLine> lines)
    {
        var c = await ConnAsync();
        await c.InsertAsync(sale);
        var list = lines.ToList();
        foreach (var l in list) l.SaleId = sale.Id;
        if (list.Count > 0) await c.InsertAllAsync(list);
        return sale.Id;
    }

    public async Task<List<Sale>> GetSalesAsync(int userId)
    {
        var c = await ConnAsync();
        return await c.Table<Sale>().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedUtc).ToListAsync();
    }

    public async Task<Sale?> GetSaleAsync(int id)
    {
        var c = await ConnAsync();
        return await c.Table<Sale>().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<SaleLine>> GetSaleLinesAsync(int saleId)
    {
        var c = await ConnAsync();
        return await c.Table<SaleLine>().Where(x => x.SaleId == saleId).ToListAsync();
    }
}
