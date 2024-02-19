using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OTD.Variant.Manager.UX.ViewModels.Menus;

#nullable enable

public partial class MenuEntryViewModel : ViewModelBase
{
    #region Observable fields

    [ObservableProperty]
    private string _label = string.Empty;

    #endregion

    #region Properties

    public MenuEntryViewModel()
    {
    }

    public MenuEntryViewModel(string label)
    {
        Label = label;
    }

    #endregion

    #region Events

    public event EventHandler? Clicked;

    #endregion

    #region Methods

    public void OnClicked()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}