using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Procedures
{
    public interface IGameDownloaderProcedures
    {
        public delegate void FileDownloadChanged(string file);

        public delegate void ProgressDownloadChanged(object sender, ProgressChangedEventArgs e);

        public event FileDownloadChanged FileChanged;
        public event ProgressDownloadChanged ProgressChanged;

        Task<string> DownloadGame(string version, GameLoader loader, OsType osType, string osArch);
        Task<bool> IsFullLoaded(IGameProfile baseProfile, IStartupOptions startupOptions = null);

        Task<Process> CreateProfileProcess(IGameProfile baseProfile, IStartupOptions startupOptions, IUser user,
            bool forceDownload, string[]? jvmArguments);

        Task<bool> CheckClientExists(IGameProfile baseProfile);
        Task<bool> CheckOsTypeLoaded(IGameProfile baseProfile, IStartupOptions startupOptions);
    }
}
