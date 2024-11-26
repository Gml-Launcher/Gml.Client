using System;

namespace GmlCore.Interfaces.Sentry;

[Flags]
public enum ProjectType
{
    Launcher = 1,
    Profiles = 2,
    Backend = 4,
    All = Launcher | Profiles | Backend
}
