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
            .Select(device => new MenuEntryViewModel(device));

        _currentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(_deviceEntries);

        _variantSelectionScreenViewModel = new VariantSelectionViewModel(variantRepository);

        InitializeEvents(true);
        StartSelection(manufacturer);

        NextViewModel = this;
        CanGoBack = true;
    }

    private void InitializeEvents(bool doInitBackRequested = false)
    {
        foreach (var entry in CurrentDeviceEntries)
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
            OnSearchTextChanged(value);
        }
    }

    #endregion

    #region Methods

    public void StartSelection(string manufacturer)
    {
        UnsubscribeEvents();

        Manufacturer = manufacturer;
        Path = $"Manufacturers > {Manufacturer}";

        _deviceEntries = _variantRepository.GetDevices(manufacturer)
            .Select(device => new MenuEntryViewModel(device));

        CurrentDeviceEntries = new ObservableCollection<MenuEntryViewModel>(_deviceEntries);

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

        UnsubscribeEvents();
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
            // Gather the variant entries and then set the current variant entries in the variant selection view model.
            var variants = _variantRepository.GetVariants(Manufacturer, entry.Label);
            var variantEntries = variants.Select(variant => new MenuEntryViewModel(variant));
            var variantEntriesCollection = new ObservableCollection<MenuEntryViewModel>(variantEntries);

            // then set the current variant entries in the variant selection view model.
            VariantSelectionScreenViewModel.CurrentVariantEntries = variantEntriesCollection;
            VariantSelectionScreenViewModel.StartSelection(Manufacturer, entry.Label);

            Dispatcher.UIThread.Post(() => NextViewModel = VariantSelectionScreenViewModel);
        }
    }

    #endregion

    #region Disposal

    public void UnsubscribeEvents(bool doDisposeBackRequested = false)
    {
        foreach (var entry in CurrentDeviceEntries)
        {
            entry.Clicked -= OnDeviceEntryClicked;
        }

        if (doDisposeBackRequested)
            VariantSelectionScreenViewModel.BackRequested -= OnBackRequested;
    }

    #endregion
}