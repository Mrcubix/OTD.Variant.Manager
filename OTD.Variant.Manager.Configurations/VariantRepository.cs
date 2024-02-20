using System.Collections.ObjectModel;
using OpenTabletDriver.Plugin.Tablet;

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
            .Select(variant => variant.Name.Replace(device, string.Empty)
                                           .Replace(manufacturer, string.Empty)
                                           .Trim());                   
    }

    /// <summary>
    ///   Get the configuration for the specified manufacturer, device, and variant.
    /// </summary>
    /// <param name="manufacturer"> The name of the manufacturer. </param>
    /// <param name="device"> The name of the device. </param>
    /// <param name="variant"> The name of the variant. </param>
    /// <returns> The configuration for the specified manufacturer, device, and variant. </returns>
    public TabletConfiguration? GetConfiguration(string manufacturer, string device, string variant)
    {
        return _configurationProvider.GetConfiguration(manufacturer, device, variant);
    }

    /// <summary>
    ///   Get the configuration variant for the specified manufacturer, device, and variant.
    /// </summary>
    /// <param name="manufacturer"> The name of the manufacturer. </param>
    /// <param name="device"> The name of the device. </param>
    /// <param name="variant"> The name of the variant. </param>
    /// <returns> The configuration variant for the specified manufacturer, device, and variant. </returns>
    public TabletVariant? GetTabletVariant(string manufacturer, string device, string variant)
    {
        return _configurationProvider.GetConfigurationVariant(manufacturer, device, variant);
    }

    #endregion
}