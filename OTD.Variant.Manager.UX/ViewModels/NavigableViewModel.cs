using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OTD.Variant.Manager.UX.ViewModels;

#nullable enable

public abstract partial class NavigableViewModel : ViewModelBase
{
    public abstract event EventHandler? BackRequested;

    public bool CanGoBack { get; init; }

    [ObservableProperty]
    protected NavigableViewModel? _nextViewModel = null;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    protected abstract void GoBack();
}