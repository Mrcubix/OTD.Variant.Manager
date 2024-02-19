using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OTD.Variant.Manager.Configurations;
using OTD.Variant.Manager.UX.ViewModels.Menus;

namespace OTD.Variant.Manager.UX.ViewModels;

#nullable enable

public partial class MainViewModel : NavigableViewModel
{
    #region Constructors

    public MainViewModel()
    {
        NextViewModel = new ManufacturerSelectionViewModel();
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
