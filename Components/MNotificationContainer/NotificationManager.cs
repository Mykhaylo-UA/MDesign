using System;
using System.Collections.Generic;
using System.Linq;
using DesignLibrary.Components.MNotification;
namespace DesignLibrary.Components.MNotificationContainer;

public class NotificationManager
{
    public List<MNotificationModel> Notifications { get; } = new();

    List<MNotificationModel> RemoveNotifications { get; } = new();

    public event Action NotifyUpdate;

    public void AddNotification(string text, MNotificationType type = MNotificationType.Info, int time = 0)
    {
        Guid newGuid = Guid.NewGuid();
        Notifications.Add(new MNotificationModel(){Text = text, Id = newGuid, Type = type, Time = time});
        NotifyUpdate?.Invoke();
    }
    public void DeleteNotification(Guid id)
    {
        MNotificationModel notification = Notifications.FirstOrDefault(n => n.Id == id);
        if (notification is not null)
            RemoveNotifications.Add(Notifications.First(n => n.Id == id));
        
        if (Notifications.Count != RemoveNotifications.Count) return;
        
        Notifications.RemoveRange(0, Notifications.Count);
        RemoveNotifications.RemoveRange(0, RemoveNotifications.Count);
        NotifyUpdate?.Invoke();
    }
}