using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HidSharp;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OTD.Variant.Manager.Configurations;

namespace OTD.Variant.Manager.UX.ViewModels.Menus;

#nullable enable

public partial class DeviceSelectionViewModel : NavigableViewModel
{
    #region Non-observable fields

    private string _searchText = string.Empty;

    private List<MenuEntryViewModel> _deviceEntries;

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
    private ObservableCollection<MenuEntryViewModel> _currentDeviceEntries;

    #region Screens

    [ObservableProperty]
    private VariantSelectionViewModel _variantSelectionScreenViewModel = new();

    #endregion

    #endregion

    // Design-time constructor.
    public DeviceSelectionViewModel() : this(null!) {}

    public DeviceSelectionViewModel(VariantRepository variantRepository)
    {
        _manufacturer = "x";
        _path = "Manufacturers > x";

        _variantRepository = variantRepository;
        _deviceEntries = new List<MenuEntryViewModel>();
        _currentDeviceEntries = new ObservableCollection<MenuEntryViewModel>();

        _variantSelectionScreenViewModel = new VariantSelectionViewModel(variantRepository);

        InitializeEvents(true);

        NextViewModel = this;
        CanGoBack = true;
    }

    public DeviceSelectionViewModel(string manufacturer, VariantRepository variantRepository)
    {
        _manufacturer = manufacturer;
        _path = $"Manufacturers > {manufacturer}";

        _variantRepository = variantRepository;

        _deviceEntries = _variantRepository.GetDevices(manufacturer)
            .Select(device => new MenuEntryViewModel(device)).ToList();

        _currentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(_deviceEntries);

        _variantSelectionScreenViewModel = new VariantSelectionViewModel(variantRepository);

        InitializeEvents(true);
        StartSelection(manufacturer);

        NextViewModel = this;
        CanGoBack = true;
    }

    private void InitializeEvents(bool doInitBackRequested = false)
    {
        foreach (var entry in _deviceEntries)
        {
            entry.Clicked += OnDeviceEntryClicked;
        }

        if (doInitBackRequested)
            VariantSelectionScreenViewModel.BackRequested += OnBackRequested;
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
            OnPropertyChanged(nameof(SearchText));

            OnSearchTextChanged(value);
        }
    }

    #endregion

    #region Methods

    public void StartSelection(string manufacturer)
    {
        // Selected manufacturer has changed.
        if (Manufacturer != manufacturer)
        {
            // Unsubscribe events on previous device entries.
            UnsubscribeEvents();
        
            Manufacturer = manufacturer;
            Path = $"Manufacturers > {Manufacturer}";

            // Reset search text.
            SearchText = string.Empty;

            // Gather devices into entries and set the current device entries.
            _deviceEntries = _variantRepository.GetDevices(manufacturer)
                .Select(device => new MenuEntryViewModel(device)).ToList();

            // Set the current device entries.
            CurrentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(_deviceEntries);

            // Show a random device in the search bar watermark.
            var random = new Random();
            var randomIndex = random.Next(CurrentDeviceEntries.Count);

            // Select a random manufacturer to be featured in the watermark.
            SearchBarWatermark = $"Search \"{CurrentDeviceEntries[randomIndex].Label}\"...";

            // Subscribe events on new device entries.
            InitializeEvents();
        }
    }

    #region Parent Implementation

    protected override void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #endregion

    #region Event Handlers

    private void OnBackRequested(object? sender, EventArgs e)
    {
        if (sender is VariantSelectionViewModel)
        {
            NextViewModel = this;
        }
    }

    private void OnSearchTextChanged(string searchText)
    {
        CurrentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(
            _deviceEntries.Where(entry => entry.Label.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
    }

    private void OnDeviceEntryClicked(object? sender, EventArgs e)
    {
        if (sender is MenuEntryViewModel entry)
        {
            VariantSelectionScreenViewModel.StartSelection(Manufacturer, entry.Label);

            //Dispatcher.UIThread.Post(() => NextViewModel = VariantSelectionScreenViewModel);
            NextViewModel = VariantSelectionScreenViewModel;
        }
    }

    #endregion

    #region Disposal

    public void UnsubscribeEvents(bool doDisposeBackRequested = false)
    {
        foreach (var entry in _deviceEntries)
        {
            entry.Clicked -= OnDeviceEntryClicked;
        }

        if (doDisposeBackRequested)
            VariantSelectionScreenViewModel.BackRequested -= OnBackRequested;
    }

    #endregion
}