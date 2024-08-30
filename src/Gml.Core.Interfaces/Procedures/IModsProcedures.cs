using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Mods;

namespace GmlCore.Interfaces.Procedures;

public interface IModsProcedures
{
    Task<IEnumerable<IMod>> GetModsAsync(IGameProfile profile);
    Task<IEnumerable<IMod>> GetModsAsync(IGameProfile profile, string name);
}
