using GmlCore.Interfaces.Integrations;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Procedures;

namespace GmlCore.Interfaces
{
    public interface IGmlManager
    {
        public ILauncherInfo LauncherInfo { get; }
        public IProfileProcedures Profiles { get; }
        public IFileStorageProcedures Files { get; }
        public IServicesIntegrationProcedures Integrations { get; }
        public IUserProcedures Users { get; }
        public ILauncherProcedures Launcher { get; }
        IProfileServersProcedures Servers { get; }
        INotificationProcedures Notifications { get; }
    }
}
