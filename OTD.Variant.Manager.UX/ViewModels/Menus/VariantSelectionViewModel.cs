using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OTD.Variant.Manager.Configurations;
using ReactiveUI;
using IOPath = System.IO.Path;

namespace OTD.Variant.Manager.UX.ViewModels.Menus;

#nullable enable

public partial class VariantSelectionViewModel : NavigableViewModel
{
    #region Non-observable fields

    private static readonly string currentLocation = Assembly.GetExecutingAssembly().Location;

    private static readonly FileInfo errorLogFileInfo = new(currentLocation);

    private readonly string errorLogLocation = IOPath.Combine(errorLogFileInfo.Directory!.FullName, "error.log");

    private string _searchText = string.Empty;

    private IEnumerable<MenuEntryViewModel> _variantEntries;

    private VariantRepository _variantRepository;

    private RpcClient<IDriverDaemon> _driverDaemon;

    private JsonSerializer _serializer = new()
    {
        Formatting = Formatting.Indented
    };

    #endregion

    #region Observable fields

    [ObservableProperty]
    private string _manufacturer;

    [ObservableProperty]
    private string _device;

    [ObservableProperty]
    private string _path = "Manufacturers > x > y";

    [ObservableProperty]
    private string _searchBarWatermark = "Search...";

    [ObservableProperty]
    private ObservableCollection<MenuEntryViewModel> _currentVariantEntries = new();

    #endregion

    // Design-time constructor.
    public VariantSelectionViewModel() : this(null!) { }

    public VariantSelectionViewModel(VariantRepository variantRepository)
    {
        _manufacturer = "x";
        _device = "y";
        _path = "Manufacturers > x > y";

        _variantRepository = variantRepository;
        _variantEntries = new List<MenuEntryViewModel>();
        _driverDaemon = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");

        NextViewModel = this;
        CanGoBack = true;
    }

    public VariantSelectionViewModel(string manufacturer, string device, VariantRepository variantRepository, RpcClient<IDriverDaemon>? driverDaemon)
    {
        _manufacturer = manufacturer;
        _device = device;
        _path = $"Manufacturers > {manufacturer} > {device}";

        _variantRepository = variantRepository;
        _variantEntries = new List<MenuEntryViewModel>();
        _driverDaemon = driverDaemon!;

        StartSelection(manufacturer, device);
        AttemptConnection();

        NextViewModel = this;
        CanGoBack = true;
    }

    private void InitializeEvents()
    {
        SubscribeEvents();

        if (_driverDaemon != null)
            _driverDaemon.Disconnected += OnDriverDisconnected;
    }

    private void SubscribeEvents()
    {
        foreach (var entry in CurrentVariantEntries)
        {
            entry.Clicked += OnVariantEntryClicked;
        }
    }

    private void AttemptConnection()
    {
        if (_driverDaemon == null)
            _driverDaemon = new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon");

        if (!_driverDaemon.IsConnected)
            _ = _driverDaemon.Connect();
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

    // TODO: YES / NO dialog
    public Interaction<Unit, bool> ConfirmationDialog { get; } = new();

    // TODO: OK dialog
    public Interaction<string, Unit> SuccessDialog { get; } = new();

    // TODO: Unfortunate / Open Error Log dialog
    public Interaction<string, Unit> ErrorDialog { get; } = new();

    #endregion

    #region Methods

    #region Variant Selection

    public void StartSelection(string manufacturer, string device)
    {
        Manufacturer = manufacturer;
        Device = device;
        Path = $"Manufacturers > {manufacturer} > {device}";

        _variantEntries = _variantRepository.GetVariants(manufacturer, device)
            .Select(variant => new MenuEntryViewModel(variant));

        CurrentVariantEntries = new ObservableCollection<MenuEntryViewModel>(_variantEntries);

        var random = new Random();
        var randomIndex = random.Next(CurrentVariantEntries.Count);

        // Select a random manufacturer to be featured in the watermark.
        SearchBarWatermark = $"Search \"{CurrentVariantEntries[randomIndex].Label}\"...";
        AttemptConnection();

        SubscribeEvents();
    }

    #endregion

    #region Variant Installation

    [RelayCommand]
    public async Task OpenConfirmationDialogAsync(MenuEntryViewModel entry)
    {
        var result = await ConfirmationDialog.Handle(Unit.Default).ToTask();

        if (result == true)
        {
            // check if the driver is connected
            if (_driverDaemon == null || _driverDaemon.IsConnected == false)
            {
                var message = "The driver is not connected. Please connect the driver and try again.";
                File.WriteAllText(errorLogLocation, message);
                await ErrorDialog.Handle(message);

                return;
            }

            // attempt to obtain the appinfo
            var appInfo = await GetAppInfoAsync();

            if (appInfo == null)
            {
                var message = "The driver is not providing the configuration directory. Please try again.";
                File.WriteAllText(errorLogLocation, message);
                await ErrorDialog.Handle(message);

                return;
            }

            if (await CompleteJobAsync(entry, appInfo))
            {
                _ = await SuccessDialog.Handle("The configuration has been successfully installed.");
            }
        }
    }

    private async Task<AppInfo?> GetAppInfoAsync()
    {
        if (_driverDaemon == null || !_driverDaemon.IsConnected)
            return null;

        try
        {
            return await _driverDaemon.Instance.GetApplicationInfo();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<bool> CompleteJobAsync(MenuEntryViewModel entry, AppInfo appInfo)
    {
        var configDirectoryPath = appInfo.ConfigurationDirectory;

        // We create Neccessary directories and files if they don't exist.
        try
        {
            if (!await EnsureFilesExistanceAsync(configDirectoryPath))
                return false;
        }
        catch (Exception e)
        {
            File.WriteAllText(errorLogLocation, e.Message);
            _ = await ErrorDialog.Handle($"An error occured while trying to ensure the existance of the configuration files, check the error.log file for more information.");

            return false;
        }

        var newConfigVariant = _variantRepository.GetTabletVariant(Manufacturer, Device, entry.Label);

        if (newConfigVariant == null)
        {
            var message = "The selected variant's configuration is not available.";
            File.WriteAllText(errorLogLocation, message);
            _ = await ErrorDialog.Handle(message);

            return false;
        }

        try
        {
            PerformBackup(configDirectoryPath);
        }
        catch (Exception e)
        {
            File.WriteAllText(errorLogLocation, e.Message);
            _ = await ErrorDialog.Handle($"An error occured while trying to perform a backup of the old configuration, check the error.log file for more information.");

            return false;
        }

        // Old config might have been moved, if stock has been selected, then nothing needs to be done.
        if (!newConfigVariant.IsStock)
        {
            try
            {
                WriteNewConfig(newConfigVariant, configDirectoryPath);
            }
            catch (Exception e)
            {
                File.WriteAllText(errorLogLocation, e.Message);
                _ = await ErrorDialog.Handle($"An error occured while trying to write the new configuration");

                return false;
            }
        }

        return true;
    }

    private async Task<bool> EnsureFilesExistanceAsync(string configDirectoryPath)
    {
        if (string.IsNullOrEmpty(configDirectoryPath))
        {
            // TODO: Notify the user that the driver's configuration directory is not being provided by the driver.
            return false;
        }

        var configDirectory = new DirectoryInfo(configDirectoryPath);

        if (!configDirectory.Exists)
        {
            if (configDirectory.Parent == null)
            {
                File.WriteAllText(errorLogLocation, "The path provided by the driver is not valid.");
                _ = await ErrorDialog.Handle("The path provided by the driver is not valid");

                return false;
            }
            else
            {
                configDirectory.Create();
            }
        }

        var manufacturerDirectory = IOPath.Combine(configDirectoryPath, Manufacturer);

        if (!Directory.Exists(manufacturerDirectory))
            Directory.CreateDirectory(IOPath.Combine(configDirectoryPath, Manufacturer));

        return true;
    }

    private void PerformBackup(string configDirectoryPath)
    {
        var configDirectory = new DirectoryInfo(configDirectoryPath);

        if (!configDirectory.Exists)
            configDirectory.Create();

        var manufacturerDirectory = IOPath.Combine(configDirectoryPath, Manufacturer);

        if (!Directory.Exists(manufacturerDirectory))
            Directory.CreateDirectory(IOPath.Combine(configDirectoryPath, Manufacturer));

        var oldConfigPath = IOPath.Combine(manufacturerDirectory, $"{Device}.json");
        var oldConfig = new FileInfo(oldConfigPath);

        if (oldConfig.Exists)
        {
            var backupDirectory = IOPath.Combine(configDirectory.Parent!.FullName, "Backups");

            if (!Directory.Exists(backupDirectory))
                Directory.CreateDirectory(backupDirectory);

            var backupManufacturerDirectory = IOPath.Combine(backupDirectory, Manufacturer);

            if (!Directory.Exists(backupManufacturerDirectory))
                Directory.CreateDirectory(backupManufacturerDirectory);

            var timestampString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            var backupConfigPath = IOPath.Combine(backupManufacturerDirectory, $"{timestampString}_{Device}.json");

            oldConfig.MoveTo(backupConfigPath);
        }
    }

    private void WriteNewConfig(TabletVariant variant, string configDirectoryPath)
    {
        var newConfigPath = IOPath.Combine(configDirectoryPath, Manufacturer, $"{Device}.json");
        var newConfig = new FileInfo(newConfigPath);

        if (newConfig.Exists)
            newConfig.Delete();

        using var stream = newConfig.Create();

        var writer = new JsonTextWriter(new StreamWriter(stream));

        _serializer.Serialize(writer, variant.Configuration);

        writer.Flush();
        stream.Close();
    }

    #endregion

    #region Parent Implementation

    protected override void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);

        UnsubscribeEvents();
    }

    #endregion

    #endregion

    #region Event Handlers

    private void OnDriverDisconnected(object? sender, EventArgs e)
    {
        // TODO: Notify the user that the driver has disconnected.
    }

    private void OnSearchTextChanged(string searchText)
    {
        CurrentVariantEntries = new ObservableCollection<MenuEntryViewModel>(
            _variantEntries.Where(entry => entry.Label.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
    }

    // It's time to prompt the user on whether or not they want to proceed with the selected variant.
    private void OnVariantEntryClicked(object? sender, EventArgs e)
    {
        if (sender is MenuEntryViewModel entry)
        {
            _ = Task.Run(() => OpenConfirmationDialogAsync(entry));
        }
    }

    #endregion

    #region Disposal

    public void UnsubscribeEvents(bool doDisposeBackRequested = false)
    {
        foreach (var entry in CurrentVariantEntries)
        {
            entry.Clicked -= OnVariantEntryClicked;
        }
    }

    #endregion
}