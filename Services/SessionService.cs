using System.Security.Cryptography;
using System.Text;
using ScanSale.Models;

namespace ScanSale.Services;

/// <summary>
/// Offline authentication + current-session holder. There is no server: the first
/// time a username is used it is created locally with the given PIN; thereafter the
/// PIN must match. The raw PIN is never stored — only a salted SHA-256 hash.
/// </summary>
public class SessionService
{
    private readonly DatabaseService _db;
    public SessionService(DatabaseService db) => _db = db;

    public AppUser? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;
    public int CurrentUserId => CurrentUser?.Id ?? 0;

    /// <summary>
    /// Signs in an existing profile (PIN must match) or creates a new one if the
    /// username is unused. Returns an error message on failure, null on success.
    /// </summary>
    public async Task<string?> SignInOrCreateAsync(string username, string pin)
    {
        username = (username ?? string.Empty).Trim();
        pin = (pin ?? string.Empty).Trim();
        if (username.Length < 2) return "Enter a username (at least 2 characters).";
        if (pin.Length < 4) return "Enter a PIN of at least 4 digits.";

        var existing = await _db.GetUserByUsernameAsync(username);
        var hash = Hash(username, pin);

        if (existing is null)
        {
            CurrentUser = await _db.CreateUserAsync(username, hash);
            return null;
        }

        if (!string.Equals(existing.PinHash, hash, StringComparison.Ordinal))
            return "Incorrect PIN for this username.";

        CurrentUser = existing;
        return null;
    }

    public void SignOut() => CurrentUser = null;

    private static string Hash(string username, string pin)
    {
        var bytes = Encoding.UTF8.GetBytes($"{username.ToLowerInvariant()}:{pin}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
