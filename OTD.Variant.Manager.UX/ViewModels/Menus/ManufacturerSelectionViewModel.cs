using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OTD.Variant.Manager.Configurations;

namespace OTD.Variant.Manager.UX.ViewModels.Menus;

#nullable enable

/// <summary>
///   The view model for the manufacturer selection screen.
/// </summary>
public partial class ManufacturerSelectionViewModel : NavigableViewModel
{
    #region Non-observable fields

    private string _searchText = string.Empty;

    private List<MenuEntryViewModel> _manufacturerEntries;

    private VariantRepository _variantRepository;

    #endregion

    #region Observable fields

    [ObservableProperty]
    private string _searchBarWatermark = "Search...";

    [ObservableProperty]
    private ObservableCollection<MenuEntryViewModel> _currentManufacturerEntries = new();

    #region Screens

    [ObservableProperty]
    private DeviceSelectionViewModel _deviceSelectionScreenViewModel;

    #endregion

    #endregion

    #region Initializers

    public ManufacturerSelectionViewModel()
    {
        var pluginConfigurationProvider = new PluginConfigurationProvider();
        _variantRepository = new VariantRepository(pluginConfigurationProvider);

        _manufacturerEntries = _variantRepository.GetManufacturers()
            .Select(manufacturer => new MenuEntryViewModel(manufacturer)).ToList();

        SubscriveEvents();

        _currentManufacturerEntries = new ObservableCollection<MenuEntryViewModel>(_manufacturerEntries);

        var random = new Random();
        var randomIndex = random.Next(_currentManufacturerEntries.Count);

        // Select a random manufacturer to be featured in the watermark.
        SearchBarWatermark = $"Search \"{_currentManufacturerEntries[randomIndex].Label}\"...";

        _deviceSelectionScreenViewModel = new DeviceSelectionViewModel(_variantRepository);
        _deviceSelectionScreenViewModel.BackRequested += OnBackRequested;
        
        NextViewModel = this;
        CanGoBack = false;
    }

    private void SubscriveEvents()
    {
        foreach (var entry in _manufacturerEntries)
        {
            entry.Clicked += OnManufacturerEntryClicked;
        }
    }

    #endregion

    #region Events

    public override event EventHandler? BackRequested;

    #endregion

    #region Properties

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            OnPropertyChanged(nameof(SearchText));

            OnSearchTextChanged(value);
        }
    }

    #endregion

    #region Parent Implementation

    protected override void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Event Handlers

    private void OnBackRequested(object? sender, EventArgs e)
    {
        if (NextViewModel is DeviceSelectionViewModel)
        {
            NextViewModel = this;
        }
    }

    private void OnSearchTextChanged(string searchText)
    {
        CurrentManufacturerEntries = new ObservableCollection<MenuEntryViewModel>(
            _manufacturerEntries.Where(entry => entry.Label.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
    }

    private void OnManufacturerEntryClicked(object? sender, EventArgs e)
    {
        if (sender is MenuEntryViewModel entry)
        {
            DeviceSelectionScreenViewModel.StartSelection(entry.Label);
            NextViewModel = DeviceSelectionScreenViewModel;
        }
    }

    #endregion
}