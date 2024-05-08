using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Procedures;
using GmlCore.Interfaces.System;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Launcher
{
    public interface IGameProfile : IDisposable
    {
        [JsonIgnore] IProfileProcedures ProfileProcedures { get; set; }
        [JsonIgnore] IGameDownloaderProcedures GameLoader { get; set; }

        string Name { get; set; }
        string GameVersion { get; set; }
        string LaunchVersion { get; set; }
        GameLoader Loader { get; }
        string ClientPath { get; set; }
        string IconBase64 { get; set; }
        string Description { get; set; }
        List<IFileInfo>? FileWhiteList { get; set; }
        DateTimeOffset CreateDate { get; set; }

        Task<bool> ValidateProfile();
        Task<bool> CheckIsFullLoaded(IStartupOptions startupOptions);
        Task Remove();
        Task DownloadAsync(OsType startupOptionsOsType, string startupOptionsOsArch);
        Task<Process> CreateProcess(IStartupOptions startupOptions, IUser user);
        Task<bool> CheckClientExists();
        Task<bool> CheckOsTypeLoaded(IStartupOptions startupOptions);
        Task<string[]> InstallAuthLib();
        Task<IGameProfileInfo?> GetCacheProfile();
    }
}
