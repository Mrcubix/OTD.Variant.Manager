using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using OTD.Variant.Manager.UX.ViewModels;
using OTD.Variant.Manager.UX.ViewModels.Windows;
using ReactiveUI;

namespace OTD.Variant.Manager.UX.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private static readonly string currentLocation = Assembly.GetExecutingAssembly().Location;

    private static readonly FileInfo errorLogFileInfo = new(currentLocation);

    private readonly string errorLogLocation = Path.Combine(errorLogFileInfo.Directory!.FullName, "error.log");

    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(action => 
            action(ViewModel!.ManufacturerSelectionViewModel
                             .DeviceSelectionScreenViewModel
                             .VariantSelectionScreenViewModel
                             .ConfirmationDialog.RegisterHandler(DoShowConfirmationDialogAsync)));

        this.WhenActivated(action =>
            action(ViewModel!.ManufacturerSelectionViewModel
                             .DeviceSelectionScreenViewModel
                             .VariantSelectionScreenViewModel
                             .ErrorDialog.RegisterHandler(DoShowErrorDialogAsync)));

        this.WhenActivated(action =>
            action(ViewModel!.ManufacturerSelectionViewModel
                             .DeviceSelectionScreenViewModel
                             .VariantSelectionScreenViewModel
                             .SuccessDialog.RegisterHandler(DoShowSuccessDialogAsync)));

    }

    #region Confirmation Dialog

    public async Task DoShowConfirmationDialogAsync(InteractionContext<Unit, bool> interaction)
    {
        await Dispatcher.UIThread.Invoke(async () => await DoShowConfirmationDialogCoreAsync(interaction));
    }

    private async Task DoShowConfirmationDialogCoreAsync(InteractionContext<Unit, bool> interaction)
    {
        var dialog = new TwoChoiceDialogWindow();
        {
            dialog.DataContext = new ConfirmationWindowViewModel();
        }

        var result = await dialog.ShowDialog<bool>(this);
        interaction.SetOutput(result);
    }

    #endregion

    #region Error Dialog

    public async Task DoShowErrorDialogAsync(InteractionContext<string, Unit> interaction)
    {
        await Dispatcher.UIThread.Invoke(async () => await DoShowErrorDialogCoreAsync(interaction));
    }

    private async Task DoShowErrorDialogCoreAsync(InteractionContext<string, Unit> interaction)
    {
        var dialog = new TwoChoiceDialogWindow();
        {
            dialog.DataContext = new ErrorWindowViewModel()
            {
                Content = interaction.Input
            };
        }

        var result = await dialog.ShowDialog<bool>(this);

        if (result)
        {
            if (OperatingSystem.IsWindows())
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = errorLogLocation,
                        UseShellExecute = true,
                    }
                }.Start();
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", errorLogLocation);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", errorLogLocation);
            }
        }

        interaction.SetOutput(Unit.Default);
    }

    #endregion

    #region Success Dialog

    public async Task DoShowSuccessDialogAsync(InteractionContext<string, Unit> interaction)
    {
        await Dispatcher.UIThread.Invoke(async () => await DoShowSuccessDialogCoreAsync(interaction));
    }

    private async Task DoShowSuccessDialogCoreAsync(InteractionContext<string, Unit> interaction)
    {
        var dialog = new OneButtonDialogWindow();
        {
            dialog.DataContext = new OneButtonWindowViewModel()
            {
                Content = interaction.Input
            };
        }

        await dialog.ShowDialog<bool>(this);
        interaction.SetOutput(Unit.Default);
    }

    #endregion
}