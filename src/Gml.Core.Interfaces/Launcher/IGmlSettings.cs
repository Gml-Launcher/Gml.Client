using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Procedures;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Interfaces.Launcher
{
    public interface IGmlSettings
    {
        public string Name { get; }
        public string BaseDirectory { get; }
        public string InstallationDirectory { get; }
        IStorageSettings StorageSettings { get; set; }
        string SecurityKey { get; set; }
        ISystemProcedures SystemProcedures { get; }
    }
}
