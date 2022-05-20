using Microsoft.AspNetCore.Components;

namespace DesignLibrary.Components.MModal;

public partial class MModal : ComponentBase
{
    [Parameter] public string Title { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public bool Show { get; set; }
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }

    void CloseModal()
    {
        ShowChanged.InvokeAsync(false);
    }
}