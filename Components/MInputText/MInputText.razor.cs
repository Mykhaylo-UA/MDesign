using Microsoft.AspNetCore.Components;

namespace DesignLibrary.Components.MInputText;

public partial class MInputText : ComponentBase
{
    [Parameter] public string Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string Placeholder { get; set; }
    [Parameter] public string Style { get; set; }

    async void OnChangeInternal(ChangeEventArgs args)
    {
        await ValueChanged.InvokeAsync($"{args.Value}");
    }
}
