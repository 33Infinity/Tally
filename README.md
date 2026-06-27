# ScanSale

A cross-platform (.NET MAUI) **offline** point-of-sale companion. Each user keeps a
local catalog of items (with QR codes) and rings up sales by scanning those QR
codes — all on-device, no server, no POS integration.

## Features
1. **Local login** — username + PIN, created on first use, stored on-device (PIN is
   salted-SHA-256 hashed, never stored in the clear). Each user's data is private to them.
2. **Items** — add/edit/delete items with: item id/SKU, description, price, tax rate (%),
   and a QR code value.
3. **New sale** — scan item QR codes with the camera (one-to-many), or type a code as a
   fallback. Builds a cart with quantities and live subtotal / tax / total, then completes
   and records the sale.
4. **Sales history** — list of past sales with totals; tap one for its line-item detail.
5. **Fully offline** — everything is stored in a local SQLite database; nothing leaves the device.
6. No POS integration (by design, for now).

## Tech
- **.NET MAUI** (net10.0) — Android, iOS, Mac Catalyst, Windows.
- **MVVM** via `CommunityToolkit.Mvvm`.
- **Storage:** `sqlite-net-pcl` → `scansale.db3` under `FileSystem.AppDataDirectory`
  (per-app local data; on Windows: `%LOCALAPPDATA%\Packages\...` or the unpackaged app-data folder).
- **Scanning:** `ZXing.Net.Maui` (`CameraBarcodeReaderView`), registered via `.UseBarcodeReader()`.

## Project layout
```
Models/        AppUser, Item, Sale, SaleLine        (sqlite-net entities)
Services/      DatabaseService (CRUD), SessionService (login)
ViewModels/    Login, Items, ItemEdit, NewSale (+CartLine), SalesHistory, SaleDetail
Views/         matching XAML pages
Converters/    small value converters
AppShell.xaml  login route + Items/New Sale/History tab bar
MauiProgram.cs DI registration + UseBarcodeReader()
```

## Run

### Windows (built & verified)
```
dotnet build ScanSale.csproj -f net10.0-windows10.0.19041.0
dotnet run   --project ScanSale.csproj -f net10.0-windows10.0.19041.0
```
(Camera scanning uses a webcam if present; otherwise use the “type a code” box.)

### Android (built & verified)
- Start an Android emulator (or attach a device with USB debugging), then:
```
dotnet build ScanSale.csproj -f net10.0-android -t:Run
```
- Camera permission is declared in `Platforms/Android/AndroidManifest.xml`.

### iOS / Mac Catalyst (requires a Mac)
- Build/run from a Mac with Xcode (or a Windows + Mac build host in Visual Studio).
- Camera usage string is in `Platforms/iOS/Info.plist` (`NSCameraUsageDescription`).

## Notes
- **Tax** is stored per item as a **percentage rate**; each sale line computes
  `tax = round(unitPrice * rate/100, 2) * qty`.
- **Sale lines snapshot** the item (code/description/price/tax) so history stays correct
  even if the item is later edited or deleted.
- **Known warning `NU1903`**: a transitive native `SQLitePCLRaw.lib.e_sqlite3 2.1.11`
  carries a SQLite advisory; no patched package is published yet. The app is offline and
  uses only parameterized queries, so practical exposure is negligible — revisit when an
  updated bundle ships.
- First feature ideas to extend: scan-to-capture the QR when adding an item, discounts,
  receipt export, and (later) POS sync.
