using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScanSale.Models;
using ScanSale.Services;

namespace ScanSale.ViewModels;

public partial class ItemsViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SessionService _session;

    public ItemsViewModel(DatabaseService db, SessionService session)
    {
        _db = db;
        _session = session;
    }

    public ObservableCollection<Item> Items { get; } = new();

    [ObservableProperty] private bool _isEmpty;

    public async Task LoadAsync()
    {
        if (!_session.IsLoggedIn) return;
        var list = await _db.GetItemsAsync(_session.CurrentUserId);
        Items.Clear();
        foreach (var i in list) Items.Add(i);
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private Task AddAsync() => Shell.Current.GoToAsync("itemedit");

    [RelayCommand]
    private Task EditAsync(Item item) =>
        item is null ? Task.CompletedTask : Shell.Current.GoToAsync($"itemedit?id={item.Id}");

    [RelayCommand]
    private async Task SignOutAsync()
    {
        _session.SignOut();
        await Shell.Current.GoToAsync("//login");
    }
}
