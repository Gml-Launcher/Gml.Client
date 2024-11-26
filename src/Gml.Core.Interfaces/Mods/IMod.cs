using System.Collections.Generic;
using System.IO;

namespace GmlCore.Interfaces.Mods;

public interface IMod
{
    string Name { get; set; }
    string Url { get; set; }
    Stream Icon { get; set; }
    IEnumerable<string> Files { get; set; }
    IEnumerable<IMod> Dependencies { get; set; }
}
