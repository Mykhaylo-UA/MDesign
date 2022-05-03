using DesignLibrary.Components.Classes;
using DesignLibrary.Components.MButton.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DesignLibrary.Components.MButton;

public partial class MButton: ComponentBase
{
    Wave _wave = new (){X = -500, Y = -500};

    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public MButtonSize ButtonSize { get; set; } = MButtonSize.Default;
    [Parameter] public MButtonType ButtonType { get; set; } = MButtonType.Secondary;
    [Parameter] public string Style { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    async void OnClickInternal(MouseEventArgs args)
    {
        _wave = new Wave() {X = args.OffsetX, Y = args.OffsetY};
        await OnClick.InvokeAsync(args);
    }
}