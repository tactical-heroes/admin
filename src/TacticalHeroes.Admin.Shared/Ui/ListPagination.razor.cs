using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Shared.Ui;

public partial class ListPagination
{
    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public int PageSize { get; set; } = 10;

    [Parameter]
    public int TotalPages { get; set; }

    [Parameter]
    public long TotalCount { get; set; }

    [Parameter]
    public int VisibleItemCount { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> PageNumberChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> PageSizeChanged { get; set; }

    private int PageCount => Math.Max(1, TotalPages);

    private int CurrentPage => Math.Clamp(PageNumber, 1, PageCount);

    private Task ChangePageSizeAsync(int pageSize)
    {
        return PageSizeChanged.InvokeAsync(pageSize);
    }
}
