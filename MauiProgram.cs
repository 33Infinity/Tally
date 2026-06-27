using Microsoft.Extensions.Logging;
using ScanSale.Services;
using ScanSale.ViewModels;
using ScanSale.Views;
using ZXing.Net.Maui.Controls;

namespace ScanSale;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// ── Services (offline, on-device) ────────────────────────────────────
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<SessionService>();
		builder.Services.AddSingleton<SubscriptionService>();

		// ── ViewModels ───────────────────────────────────────────────────────
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<PaywallViewModel>();
		builder.Services.AddTransient<ItemsViewModel>();
		builder.Services.AddTransient<ItemEditViewModel>();
		builder.Services.AddTransient<NewSaleViewModel>();
		builder.Services.AddTransient<SalesHistoryViewModel>();
		builder.Services.AddTransient<SaleDetailViewModel>();

		// ── Pages ────────────────────────────────────────────────────────────
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<PaywallPage>();
		builder.Services.AddTransient<ItemsPage>();
		builder.Services.AddTransient<ItemEditPage>();
		builder.Services.AddTransient<NewSalePage>();
		builder.Services.AddTransient<SalesHistoryPage>();
		builder.Services.AddTransient<SaleDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
