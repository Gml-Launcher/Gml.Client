using System;

namespace GmlCore.Interfaces.User;

public interface ISession
{
    DateTimeOffset EndDate { get; set; }
    DateTimeOffset Start { get; set; }
}
