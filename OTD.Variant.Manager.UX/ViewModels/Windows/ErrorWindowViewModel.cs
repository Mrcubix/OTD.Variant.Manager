using System;

namespace OTD.Variant.Manager.UX.ViewModels.Windows;

#nullable enable

public class ErrorWindowViewModel : TwoChoiceWindowViewModel
{
    public ErrorWindowViewModel()
    {
        Title = "An Error Occured";
        Content = "An error occurred. Check the log for more details.";
        PositiveChoice = "Open Logs";
        NegativeChoice = "Close";
    }
}