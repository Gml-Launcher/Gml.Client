using System;
using GmlCore.Interfaces.User;

namespace Gml.Web.Api.Domains.User;

public class GameSession : ISession
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndDate { get; set; }

    public GameSession()
    {
        Start = DateTimeOffset.Now;
    }

}
