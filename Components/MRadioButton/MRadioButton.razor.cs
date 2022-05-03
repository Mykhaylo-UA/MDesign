using Microsoft.AspNetCore.Components;

namespace DesignLibrary.Components.MRadioButton;

public partial class MRadioButton: ComponentBase
{
    [Parameter] public string Label { get; set; }
    [Parameter] public string NameRadioGroup { get; set; }
    [Parameter] public string Style { get; set; }
}