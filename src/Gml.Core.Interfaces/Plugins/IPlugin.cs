using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;

namespace GmlCore.Interfaces.Plugins;

public interface IPlugin
{
    public Task Execute(ILauncherInfo launcherInfo);
}
