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
    Task<IExternalMod?> GetInfo(string identify, ModType modType);
    Task<IReadOnlyCollection<IModVersion>> GetVersions(IExternalMod modInfo, ModType modType, GameLoader profileLoader,
        string gameVersion);
    Task<IReadOnlyCollection<IExternalMod>> FindModsAsync(GameLoader profileLoader, string gameVersion,
        ModType modLoaderType,
        string modName,
        short take,
        short offset);
    Task SetModDetails(string modName, string title, string description);
    Task Retore();
    ICollection<IModInfo> ModsDetails { get; }
}
