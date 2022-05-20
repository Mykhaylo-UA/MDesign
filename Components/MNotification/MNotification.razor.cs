using DesignLibrary.Components.MNotificationContainer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DesignLibrary.Components.MNotification;

public partial class MNotification : ComponentBase
{
    [Inject] NotificationManager NotificationManager { get; set; }
    [Parameter] public string Text { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public Guid Id { get; set; }
    [Parameter] public int Time { get; set; }
    [Parameter] public MNotificationType Type { get; set; } = MNotificationType.Info;
    bool Remove;
    
    bool _remove = false;

    protected override async Task OnInitializedAsync()
    {
        if (Time != 0)
        {
            await Task.Delay(Time);
            Remove = true;
            StateHasChanged();

            await Task.Delay(475);
            _remove = true;
            StateHasChanged();
            NotificationManager.DeleteNotification(Id);
        }
    }


    async void OnClick(MouseEventArgs args)
    {
        Remove = true;
        StateHasChanged();

        await Task.Delay(475);
        _remove = true;
        StateHasChanged();
        NotificationManager.DeleteNotification(Id);
    }

}