using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.System;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Procedures
{
    public interface IGameDownloaderProcedures
    {
        IObservable<double> FullPercentages { get; }
        IObservable<double> LoadPercentages { get; }
        IObservable<string> LoadLog { get;}
        IObservable<Exception> LoadException { get;}

        Task<string> DownloadGame(string version, GameLoader loader);
        Task<Process> CreateProcess(IStartupOptions startupOptions, IUser user, bool needDownload, string[] jvmArguments);
        Task<IFileInfo[]> GetAllFiles();
        bool GetLauncher(string launcherKey, out object launcher);
        Task<IEnumerable<IFileInfo>> GetLauncherFiles(string osName, string osArchitecture);
    }
}
