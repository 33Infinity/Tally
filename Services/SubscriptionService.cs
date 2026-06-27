using Microsoft.Maui.Storage;
using Plugin.InAppBilling;

namespace ScanSale.Services;

/// <summary>
/// Google Play subscription gate. One product (<see cref="ProductId"/>) with a monthly
/// base plan and a 7-day free-trial offer configured in the Play Console. A trial-active
/// subscription is reported by Google as Purchased, so it counts as entitled, and Google
/// handles the trial → paid transition and renewals automatically.
///
/// Play Billing only works for builds installed from Play (e.g. the internal-testing
/// track) — never via USB/sideload. <see cref="DevBypass"/> defaults on in Debug so the
/// sideload dev loop stays usable; release builds enforce the real entitlement.
/// </summary>
public class SubscriptionService
{
    public const string ProductId = "tally_pro";

    private const string KeyActive = "tally_sub_active";
    private const string KeyChecked = "tally_sub_checked_utc";

#if DEBUG
    public bool DevBypass { get; set; } = true;
#else
    public bool DevBypass { get; set; } = false;
#endif

    public bool IsSubscribed { get; private set; }

    /// <summary>Checks Google Play for an active subscription. Falls back to the last
    /// cached state when offline so a paid user isn't locked out without a network.</summary>
    public async Task<bool> RefreshAsync()
    {
        if (DevBypass) { IsSubscribed = true; return true; }

        var billing = CrossInAppBilling.Current;
        try
        {
            if (!await billing.ConnectAsync())
                return LoadCached();

            var purchases = await billing.GetPurchasesAsync(ItemType.Subscription);
            var active = false;
            if (purchases is not null)
            {
                foreach (var p in purchases)
                {
                    if (p.ProductId != ProductId || p.State != PurchaseState.Purchased) continue;
                    active = true;
                    // Unacknowledged purchases are auto-refunded by Google after 3 days.
                    if (p.IsAcknowledged != true)
                        await billing.FinalizePurchaseAsync(new[] { p.TransactionIdentifier });
                }
            }
            Cache(active);
            IsSubscribed = active;
            return active;
        }
        catch
        {
            return LoadCached();
        }
        finally
        {
            try { await billing.DisconnectAsync(); } catch { }
        }
    }

    /// <summary>Launches the Play purchase flow (starts the free trial for an eligible
    /// account). Returns true once the subscription is active.</summary>
    public async Task<bool> PurchaseAsync()
    {
        if (DevBypass) { IsSubscribed = true; return true; }

        var billing = CrossInAppBilling.Current;
        try
        {
            if (!await billing.ConnectAsync()) return false;

            var purchase = await billing.PurchaseAsync(ProductId, ItemType.Subscription);
            var ok = purchase is not null && purchase.State == PurchaseState.Purchased;
            if (ok && purchase!.IsAcknowledged != true)
                await billing.FinalizePurchaseAsync(new[] { purchase.TransactionIdentifier });
            if (ok) { Cache(true); IsSubscribed = true; }
            return ok;
        }
        catch
        {
            return false;
        }
        finally
        {
            try { await billing.DisconnectAsync(); } catch { }
        }
    }

    /// <summary>Localized price string (e.g. "$4.99") for display, or null if unavailable.</summary>
    public async Task<string?> GetPriceAsync()
    {
        if (DevBypass) return null;

        var billing = CrossInAppBilling.Current;
        try
        {
            if (!await billing.ConnectAsync()) return null;
            var products = await billing.GetProductInfoAsync(ItemType.Subscription, new[] { ProductId });
            return products?.FirstOrDefault()?.LocalizedPrice;
        }
        catch
        {
            return null;
        }
        finally
        {
            try { await billing.DisconnectAsync(); } catch { }
        }
    }

    private bool LoadCached()
    {
        IsSubscribed = Preferences.Get(KeyActive, false);
        return IsSubscribed;
    }

    private static void Cache(bool active)
    {
        Preferences.Set(KeyActive, active);
        Preferences.Set(KeyChecked, DateTime.UtcNow.ToString("o"));
    }
}
