using Microsoft.AspNetCore.Components;

namespace DesignLibrary.Components.MCheckbox;

public partial class MCheckbox:ComponentBase
{
    [Parameter] public string Label { get; set; }
    [Parameter] public string Style { get; set; }
}