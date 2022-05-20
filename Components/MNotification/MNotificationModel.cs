using System;

namespace DesignLibrary.Components.MNotification;

public class MNotificationModel
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public int Time { get; set; }
    public MNotificationType Type { get; set; }
    public bool Remove { get; set; }
}