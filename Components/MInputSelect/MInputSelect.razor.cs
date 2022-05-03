using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DesignLibrary.Components.MInputSelect;

public partial class MInputSelect: ComponentBase
{
    [Parameter] public string Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string Placeholder { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public List<string> Values { get; set; }
    
    List<string> FilterValues { get; set; }

    bool _change = false;
    bool allowScroll = false;
    string _oldPlaceholder;
    void OnChangeInternal(ChangeEventArgs args)
    {
        string value = $"{args.Value}";
        if (!string.IsNullOrEmpty(value))
        {
            FilterValues = Values.Where(v=> v.StartsWith(value)).ToList();
            StateHasChanged();
        }
        else
        {
            FilterValues = null;
        }
    }
    async Task FocusIn()
    {
        IsOpen = true;
        _oldPlaceholder = Placeholder;
        Placeholder = Value;
        Value = "";
        _change = false;
        await Task.Delay(200);
        allowScroll = true;
        StateHasChanged();

    }
    async Task FocusOut()
    {
        IsOpen = false;
        allowScroll = false;
        if (!_change)
        {
            Value = Placeholder;
        }

        if (String.IsNullOrEmpty(Value))
        {
            Placeholder = _oldPlaceholder;
        }
        StateHasChanged();
        await Task.Delay(300);
        FilterValues = null;
    }

    async Task OnSelect(MouseEventArgs obj, string value)
    {
        Value = value;
        _change = true;
        await ValueChanged.InvokeAsync($"{value}");
    }
}