using System;
using Avalonia.ReactiveUI;
using OTD.Variant.Manager.UX.ViewModels.Windows;

namespace OTD.Variant.Manager.UX.Views;

#nullable enable

public partial class TwoChoiceDialogWindow : ReactiveWindow<TwoChoiceWindowViewModel>
{
    public TwoChoiceDialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextBeginUpdate()
    {
        base.OnDataContextBeginUpdate();

        if (DataContext is TwoChoiceWindowViewModel viewModel)
        {
            viewModel.ResultPicked += OnResultPicked;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is TwoChoiceWindowViewModel viewModel)
        {
            viewModel.ResultPicked += OnResultPicked;
        }
    }

    private void OnResultPicked(object? sender, bool e)
    {
        Close(e);
    }
}