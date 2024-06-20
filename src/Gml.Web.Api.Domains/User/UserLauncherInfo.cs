using System;
using GmlCore.Interfaces.User;

namespace Gml.Web.Api.Domains.User;

public class UserLauncherInfo
{
    public DateTimeOffset ExpiredDate { get; set; }
    public IDisposable Subscription { get; set; }
    public IUser User { get; set; }
    public dynamic Connection { get; set; }
}
