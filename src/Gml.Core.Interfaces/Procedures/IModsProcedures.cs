using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Mods;

namespace GmlCore.Interfaces.Procedures;

public interface IModsProcedures
{
    Task<IEnumerable<IMod>> GetModsAsync(IGameProfile profile);
    Task<IEnumerable<IMod>> GetModsAsync(IGameProfile profile, string name);
    Task<IExternalMod?> GetInfo(string identify);
    Task<IReadOnlyCollection<IModVersion>> GetVersions(IExternalMod modInfo, GameLoader profileLoader,
        string gameVersion);
    Task<IEnumerable<IMod>> FindModsAsync(GameLoader profileLoader, string gameVersion, string modName,
        short take,
        short offset);
    Task SetModDetails(string modName, string title, string description);
    Task Retore();
    ICollection<IModInfo> ModsDetails { get; }
}
