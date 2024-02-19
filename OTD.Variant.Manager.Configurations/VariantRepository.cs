using System.Collections.ObjectModel;

namespace OTD.Variant.Manager.Configurations;

public class VariantRepository
{
    private readonly PluginConfigurationProvider _configurationProvider;

    public VariantRepository(PluginConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;

        foreach (var manufacturer in GetManufacturers())
        {
            Manufacturers.Add(manufacturer);
        }
    }

    #region Properties

    public ObservableCollection<string> Manufacturers { get; } = new();

    public ObservableCollection<string> CachedDevices { get; } = new();

    /// <summary>
    ///   Gets the list of manufacturers.
    /// </summary>
    /// <returns> The list of manufacturers. </returns>
    public IEnumerable<string> GetManufacturers()
    {
        return _configurationProvider.ConfigurationsByManufacturer.Keys;
    }

    /// <summary>
    ///   Gets the list of devices for the specified manufacturer.
    /// </summary>
    /// <param name="manufacturer"> The name of the manufacturer. </param>
    /// <returns> The list of devices for the specified manufacturer. </returns>
    public IEnumerable<string> GetDevices(string manufacturer)
    {
        return _configurationProvider.ConfigurationsByManufacturer[manufacturer].Keys;
    }

    /// <summary>
    ///   Gets the list of variants for the specified manufacturer and device.
    /// </summary>
    /// <param name="manufacturer"> The name of the manufacturer. </param>
    /// <param name="device"> The name of the device. </param>
    /// <returns> The list of variants for the specified manufacturer and device. </returns>
    public IEnumerable<string> GetVariants(string manufacturer, string device)
    {
        return _configurationProvider.ConfigurationsByManufacturer[manufacturer][device]
            .Select(variant => variant.Name);
    }

    #endregion
}