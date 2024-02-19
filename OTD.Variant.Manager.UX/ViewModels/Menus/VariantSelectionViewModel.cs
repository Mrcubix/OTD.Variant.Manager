using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OTD.Variant.Manager.Configurations;

namespace OTD.Variant.Manager.UX.ViewModels.Menus;

#nullable enable

public partial class VariantSelectionViewModel : NavigableViewModel
{
    #region Non-observable fields

    private string _searchText = string.Empty;

    private IEnumerable<MenuEntryViewModel> _deviceEntries;

    private VariantRepository _variantRepository;

    #endregion

    #region Observable fields

    [ObservableProperty]
    private string _manufacturer;

    [ObservableProperty]
    private string _path = "Manufacturers > Placeholder";

    [ObservableProperty]
    private string _searchBarWatermark = "Search...";

    [ObservableProperty]
    private ObservableCollection<MenuEntryViewModel> _currentDeviceEntries = new();

    #endregion

    // Design-time constructor.
    public VariantSelectionViewModel()
    {
        _manufacturer = "I.";
        _path = "Manufacturers > I.";
        _variantRepository = null!;
        _deviceEntries = new List<MenuEntryViewModel>();

        NextViewModel = this;
        CanGoBack = false;
    }

    public VariantSelectionViewModel(VariantRepository variantRepository)
    {
        _manufacturer = string.Empty;
        _variantRepository = variantRepository;
        _deviceEntries = new List<MenuEntryViewModel>();

        NextViewModel = this;
        CanGoBack = true;
    }

    public VariantSelectionViewModel(string manufacturer, VariantRepository variantRepository)
    {
        _manufacturer = manufacturer;
        _path = $"Manufacturers > {manufacturer}";
        _variantRepository = variantRepository;
        _deviceEntries = new List<MenuEntryViewModel>();

        StartSelection(manufacturer);

        NextViewModel = this;
        CanGoBack = true;
    }

    private void InitializeEvents()
    {
        foreach (var entry in CurrentDeviceEntries)
        {
            entry.Clicked += OnManufacturerEntryClicked;
        }
    }

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
            OnSearchTextChanged(value);
        }
    }

    #endregion

    #region Methods

    public void StartSelection(string manufacturer)
    {
        Manufacturer = manufacturer;

        _deviceEntries = _variantRepository.GetDevices(manufacturer)
            .Select(device => new MenuEntryViewModel(device));

        CurrentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(_deviceEntries);

        Path = $"Manufacturers > {Manufacturer}";

        var random = new Random();
        var randomIndex = random.Next(CurrentDeviceEntries.Count);

        // Select a random manufacturer to be featured in the watermark.
        SearchBarWatermark = $"Search \"{CurrentDeviceEntries[randomIndex].Label}\"...";

        InitializeEvents();
    }

    #region Parent Implementation

    protected override void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #endregion

    #region Event Handlers

    private void OnSearchTextChanged(string searchText)
    {
        CurrentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(
            _deviceEntries.Where(entry => entry.Label.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
    }

    private void OnManufacturerEntryClicked(object? sender, EventArgs e)
    {
        if (sender is MenuEntryViewModel entry)
        {
            // Gather the variant entries and then set the current variant entries in the variant selection view model.
            var variants = _variantRepository.GetVariants(Manufacturer, entry.Label);
            var variantEntries = variants.Select(variant => new MenuEntryViewModel(variant));
            var variantEntriesCollection = new ObservableCollection<MenuEntryViewModel>(variantEntries);

            // then set the current variant entries in the variant selection view model.
            // _variantSelectionScreenViewModel.CurrentVariantEntries = variantEntriesCollection;
            // NextViewModel = _variantSelectionScreenViewModel;
        }
    }

    #endregion
}