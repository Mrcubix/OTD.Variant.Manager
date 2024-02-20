using System;
using Avalonia.ReactiveUI;
using OTD.Variant.Manager.UX.ViewModels.Windows;

namespace OTD.Variant.Manager.UX.Views;

#nullable enable

public partial class OneButtonDialogWindow : ReactiveWindow<TwoChoiceWindowViewModel>
{
    public OneButtonDialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextBeginUpdate()
    {
        base.OnDataContextBeginUpdate();

        if (DataContext is OneButtonWindowViewModel viewModel)
        {
            viewModel.CloseRequested += OnResultPicked;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is OneButtonWindowViewModel viewModel)
        {
            viewModel.CloseRequested += OnResultPicked;
        }
    }

    private void OnResultPicked(object? sender, bool e)
    {
        Close();
    }
}