using System.Threading.Tasks;

namespace GmlCore.Interfaces.Servers;

public interface IProfileServer
{
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    Task UpdateStatusAsync();
}
