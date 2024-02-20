using System;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OTD.Variant.Manager.UX.ViewModels.Menus;

namespace OTD.Variant.Manager.UX.ViewModels;

#nullable enable

public partial class MainViewModel : NavigableViewModel
{
    #region Observable fields

    [ObservableProperty]
    private ManufacturerSelectionViewModel _manufacturerSelectionViewModel = new();

    #endregion

    #region Constructors

    public MainViewModel()
    {
        CanGoBack = false;
    }

    #endregion

    #region Events

    public override event EventHandler? BackRequested;

    #endregion

    #region Parent Implementations

    protected override void GoBack()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
