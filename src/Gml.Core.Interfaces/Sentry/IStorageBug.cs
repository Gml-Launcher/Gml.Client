using System;

namespace GmlCore.Interfaces.Sentry;

public interface IStorageBug
{
    DateTime Date { get; set; }
    ProjectType ProjectType { get; set; }
}
