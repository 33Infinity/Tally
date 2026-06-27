using System.ComponentModel;
using ScanSale.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace ScanSale.Views;

public partial class NewSalePage : ContentPage
{
    private readonly NewSaleViewModel _vm;
    private CameraBarcodeReaderView? _scanner;

    public NewSalePage(NewSaleViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NewSaleViewModel.IsScanning))
            UpdateScanner(_vm.IsScanning);
    }

    // Create a fresh camera reader each time scanning turns on, and fully release
    // it when it turns off. A persistent/reused CameraBarcodeReaderView is the
    // cause of "preview shows but never detects" on the 2nd use.
    private void UpdateScanner(bool on)
    {
        if (on)
        {
            if (_scanner is not null) return;
            _scanner = new CameraBarcodeReaderView
            {
                Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.TwoDimensional | BarcodeFormats.OneDimensional,
                    AutoRotate = true,
                    Multiple = false
                }
            };
            _scanner.BarcodesDetected += OnBarcodesDetected;
            ScannerHost.Content = _scanner;
            _scanner.IsDetecting = true;
        }
        else
        {
            ReleaseScanner();
        }
    }

    private void ReleaseScanner()
    {
        if (_scanner is null) return;
        _scanner.IsDetecting = false;
        _scanner.BarcodesDetected -= OnBarcodesDetected;
        ScannerHost.Content = null;
        _scanner.Handler?.DisconnectHandler();
        _scanner = null;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var value = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(value)) return;
        MainThread.BeginInvokeOnMainThread(async () => await _vm.OnScannedAsync(value));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.IsScanning = false; // release the camera when leaving the page
    }
}
