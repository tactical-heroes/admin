namespace Pl.Ui.Blazor.ViewModels.Core;

public abstract record BaseViewModel<TId>
    where TId : struct
{
    public TId Id { get; set; }
}
