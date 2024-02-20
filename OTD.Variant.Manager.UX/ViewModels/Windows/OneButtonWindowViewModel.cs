using System;

namespace OTD.Variant.Manager.UX.ViewModels.Windows;

#nullable enable

public class OneButtonWindowViewModel : ViewModelBase
{
    #region Events

    public event EventHandler<bool>? CloseRequested;

    #endregion

    #region Properties

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    #endregion

    public void ReturnResult(bool result)
        => CloseRequested?.Invoke(this, result);
}