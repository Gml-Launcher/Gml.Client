using System.IO;

namespace GmlCore.Interfaces.Mods;

public interface IMod
{
    string Name { get; set; }
    string Url { get; set; }
    ModType Type { get; }
    Stream Icon { get; set; }
}

public enum ModType
{
    Local,
    Modrinth,
    CurseForge
}
