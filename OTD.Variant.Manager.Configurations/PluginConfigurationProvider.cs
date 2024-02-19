using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OTD.Variant.Manager.Configurations;

/// <summary>
///   Provides configuration files contained in the assembly.
/// </summary>
public class PluginConfigurationProvider
{
    private DirectoryInfo _sourceConfigsDirectory;

    public PluginConfigurationProvider()
    {
        // Get the location of the assembly file, within the plugin directory.
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyFile = new FileInfo(assemblyLocation);

        if (assemblyFile.Directory == null)
            throw new DirectoryNotFoundException("The directory of the assembly file is not found.");

        SourceConfigsDirectory = Path.Combine(assemblyFile.Directory.FullName, "Configurations");
        _sourceConfigsDirectory = new DirectoryInfo(SourceConfigsDirectory);

        // Check if the source directory exists.
        if (!Directory.Exists(SourceConfigsDirectory))
        {
            Log.Write("OTD Variant Manager", "How dare you remove the plugin's config folder?!", LogLevel.Error);
            return;
        }

        // Prime the json serializer.
        var serializer = new JsonSerializer();

        // Read the configuration files.
        Read(serializer);

        IsReady = true;
    }

    private void Read(JsonSerializer serializer)
    {
        // We will need the list of manufacturers and their configurations.
        var manufacturerDirectories = _sourceConfigsDirectory.GetDirectories();

        // We start by iterating through the manufacturer directories.
        foreach (var manufacturerDirectory in manufacturerDirectories)
        {
            var configsByDevice = new Dictionary<string, List<TabletVariant>>();

            // Then we iterate through the device directories, where variants are stored.
            foreach (var deviceDirectory in manufacturerDirectory.GetDirectories())
            {
                var configs = new List<TabletVariant>();

                // Need to first create the stock variant.
                var stockVariant = new TabletVariant($"{deviceDirectory.Name} Stock");
                configs.Add(stockVariant);

                // Need to first make sure we only process JSON files.
                var deviceFiles = deviceDirectory.GetFiles("*.json");

                // Finally, we iterate through the variant files.
                foreach (var variantFile in deviceFiles)
                {
                    using var stream = variantFile.OpenRead();

                    var configuration = Deserialize(serializer, stream);

                    if (configuration != null)
                    {
                        // configuration is safe to use.
                        var variant = new TabletVariant(configuration.Name, configuration);

                        configs.Add(variant);
                    }
                }

                if (configs.Count > 0)
                    configsByDevice.Add(deviceDirectory.Name, configs);
            }

            if (configsByDevice.Count > 0)
                ConfigurationsByManufacturer.Add(manufacturerDirectory.Name, configsByDevice);
        }
    }

    #region Properties

    public string SourceConfigsDirectory { get; }

    public string TargetConfigsDirectory { get; } = AppInfo.Current.ConfigurationDirectory;

    public Dictionary<string, Dictionary<string, List<TabletVariant>>> ConfigurationsByManufacturer { get; } = new();

    public bool IsReady { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///   Gets the path to the configuration file.
    /// </summary>
    /// <param name="manufacturer">The name of the manufacturer.</param>
    /// <param name="deviceName">The name of the device.</param>
    /// <param name="variantName">The name of the variant.</param>
    /// <returns>The path to the configuration file.</returns>
    /// <remarks>
    ///   If the variant name is not provided, the device name is used as the variant name.
    /// </remarks>
    public string GetConfigurationPath(string manufacturer, string deviceName, string variantName = "")
    {
        if (string.IsNullOrEmpty(variantName))
            variantName = $"{deviceName}.json";

        return Path.Combine(SourceConfigsDirectory, deviceName, variantName);
    }

    /// <summary>
    ///   Gets the configuration file.
    /// </summary>
    /// <param name="manufacturer">The name of the manufacturer.</param>
    /// <param name="deviceName">The name of the device.</param>
    /// <param name="variantName">The name of the variant.</param>
    /// <returns>The configuration file.</returns>
    public FileInfo GetConfigurationFile(string manufacturer, string deviceName, string variantName)
    {
        var path = GetConfigurationPath(manufacturer, deviceName, variantName);
        return new FileInfo(path);
    }

    /// <summary>
    ///   Gets the configuration file.
    /// </summary>
    /// <param name="manufacturer">The name of the manufacturer.</param>
    /// <param name="deviceName">The name of the device.</param>
    /// <param name="variantName">The name of the variant.</param>
    /// <returns>The configuration file.</returns>
    /// <remarks>
    ///   If the variant name is not provided, the device name is used as the variant name.
    /// </remarks>
    public TabletConfiguration? GetConfiguration(string manufacturer, string deviceName, string variantName)
    {
        if (!IsReady)
            return null;

        if (ConfigurationsByManufacturer.TryGetValue(manufacturer, out var configsByDevice))
        {
            if (configsByDevice.TryGetValue(deviceName, out var configs))
            {
                return configs.FirstOrDefault(c => c.Name == variantName)?.Configuration;
            }
        }

        return null;
    }

    #endregion

    #region Static Methods

    private static TabletConfiguration? Deserialize(JsonSerializer jsonSerializer, Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        return jsonSerializer.Deserialize<TabletConfiguration>(jsonReader);
    }

    #endregion
}