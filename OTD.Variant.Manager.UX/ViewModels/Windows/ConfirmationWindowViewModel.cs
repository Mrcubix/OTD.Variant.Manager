using System;

namespace OTD.Variant.Manager.UX.ViewModels.Windows;

#nullable enable

public class ConfirmationWindowViewModel : TwoChoiceWindowViewModel
{
    public ConfirmationWindowViewModel()
    {
        Title = "Are you sure?";
        Content = "Are you sure you want to use this variant?";
        PositiveChoice = "Yes";
        NegativeChoice = "No";
    }
}