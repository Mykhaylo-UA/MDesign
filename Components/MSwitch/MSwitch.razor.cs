using Microsoft.AspNetCore.Components;

namespace DesignLibrary.Components.MSwitch;

public partial class MSwitch : ComponentBase
{
    [Parameter] public string Label { get; set; }
    [Parameter] public string Style { get; set; }
}