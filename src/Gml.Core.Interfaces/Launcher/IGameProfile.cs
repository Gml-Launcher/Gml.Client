using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Procedures;
using GmlCore.Interfaces.Servers;
using GmlCore.Interfaces.System;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Launcher
{
    public interface IGameProfile : IDisposable
    {
        [JsonIgnore] IProfileProcedures ProfileProcedures { get; set; }
        [JsonIgnore] IProfileServersProcedures ServerProcedures { get; set; }
        [JsonIgnore] IGameDownloaderProcedures GameLoader { get; set; }

        string Name { get; set; }
        bool IsEnabled { get; set; }
        string GameVersion { get; set; }
        string? LaunchVersion { get; set; }
        GameLoader Loader { get; }
        string ClientPath { get; set; }
        string IconBase64 { get; set; }
        string BackgroundImageKey { get; set; }
        string Description { get; set; }
        List<IFileInfo>? FileWhiteList { get; set; }
        List<IProfileServer> Servers { get; set; }
        DateTimeOffset CreateDate { get; set; }
        string? JvmArguments { get; set; }
        ProfileState State { get; set; }

        Task<bool> ValidateProfile();
        Task<bool> CheckIsFullLoaded(IStartupOptions startupOptions);
        Task Remove();
        Task DownloadAsync();
        Task<Process> CreateProcess(IStartupOptions startupOptions, IUser user);
        Task<bool> CheckClientExists();
        Task<bool> CheckOsTypeLoaded(IStartupOptions startupOptions);
        Task<string[]> InstallAuthLib();
        Task<IGameProfileInfo?> GetCacheProfile();
        void AddServer(IProfileServer server);
        void RemoveServer(IProfileServer server);
        Task CreateModsFolder();
        Task<IEnumerable<IFileInfo>> GetProfileFiles(string osName, string osArchitecture);
        Task<IFileInfo[]> GetAllProfileFiles();
    }
}
