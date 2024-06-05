using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Servers;

namespace GmlCore.Interfaces.Procedures;

public interface IProfileServersProcedures
{
    Task<IProfileServer> AddMinecraftServer(IGameProfile profileprofile, string serverName, string address, int port);
    Task UpdateServerState(IProfileServer minecraftServer);
    Task RemoveServer(IGameProfile profile, string serverName);
}
