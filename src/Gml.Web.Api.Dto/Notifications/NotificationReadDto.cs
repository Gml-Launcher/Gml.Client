using System.Collections;
using System.Collections.Generic;
using GmlCore.Interfaces.Notifications;

namespace Gml.Web.Api.Dto.Notifications;

public class NotificationReadDto
{
    public IEnumerable<INotification> Notifications { get; set; }
    public int Amount { get; set; }
}
