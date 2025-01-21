using System;
using System.Collections.Generic;

namespace GmlCore.Interfaces.Mods;

public interface IModVersion
{
    string Id { get; set; }
    string Name { get; set; }
    DateTimeOffset DatePublished { get; set; }
    int Downloads { get; set; }
    string VersionName { get; set; }
}
