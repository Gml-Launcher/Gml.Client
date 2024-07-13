using System;
using GmlCore.Interfaces.Enums;

namespace GmlCore.Interfaces.Notifications;

public interface INotification
{
    public string Message { get; set; }
    public string Details { get; set; }
    public NotificationType Type { get; set; }
    public DateTimeOffset Date { get; set; }
}
