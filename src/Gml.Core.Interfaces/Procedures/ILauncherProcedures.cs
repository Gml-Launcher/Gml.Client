using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Interfaces.Procedures;

public interface ILauncherProcedures
{
    Task<string> CreateVersion(IVersionFile version);
}
