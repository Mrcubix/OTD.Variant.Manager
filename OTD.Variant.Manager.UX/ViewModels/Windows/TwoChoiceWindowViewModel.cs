using System;

namespace OTD.Variant.Manager.UX.ViewModels.Windows;

#nullable enable

public class TwoChoiceWindowViewModel : ViewModelBase
{
    #region Events

    public event EventHandler<bool>? ResultPicked;

    #endregion

    #region Properties

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string PositiveChoice { get; set; } = string.Empty;

    public string NegativeChoice { get; set; } = string.Empty;

    #endregion

    public void ReturnResult(bool result)
        => ResultPicked?.Invoke(this, result);
}