using OpenTabletDriver.Plugin.Tablet;

namespace OTD.Variant.Manager.Configurations;

public class TabletVariant
{
    public TabletVariant()
    {
        IsStock = true;
        Name = "Stock";
    }

    public TabletVariant(string name)
    {
        IsStock = true;
        Name = name;
    }

    public TabletVariant(string name, TabletConfiguration configuration)
    {
        IsStock = false;
        Name = name;
        Configuration = configuration;
    }

    public bool IsStock { get; set; }

    public string Name { get; set; } = string.Empty;

    public TabletConfiguration? Configuration { get; set; }
}