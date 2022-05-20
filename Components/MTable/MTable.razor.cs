using Microsoft.AspNetCore.Components;

namespace DesignLibrary.Components.MTable;

public partial class MTable : ComponentBase
{
    [Parameter] public RenderFragment ChildContent { get; set; }
}