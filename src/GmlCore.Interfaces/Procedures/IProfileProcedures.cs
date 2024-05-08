using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.System;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Procedures
{
    public interface IProfileProcedures
    {
        public delegate void ProgressPackChanged(ProgressChangedEventArgs e);

        bool CanUpdateAndRestore { get; }

        public event ProgressPackChanged PackChanged;
        Task AddProfile(IGameProfile? profile);

        Task<IGameProfile?> AddProfile(string name, string version, GameLoader loader, string profileIconBase64,
            string description);

        Task<bool> CanAddProfile(string name, string version);
        Task RemoveProfile(IGameProfile profile);
        Task RemoveProfile(IGameProfile profile, bool removeProfileFiles);
        Task RestoreProfiles();
        Task RemoveProfile(int profileId);
        Task ClearProfiles();
        Task<bool> ValidateProfileAsync(IGameProfile baseProfile);
        bool ValidateProfile();
        Task SaveProfiles();
        Task DownloadProfileAsync(IGameProfile baseProfile, OsType osType, string osArch);
        Task<IEnumerable<IFileInfo>> GetProfileFiles(IGameProfile baseProfile);
        Task<IGameProfile?> GetProfile(string profileName);
        Task<IEnumerable<IGameProfile>> GetProfiles();
        Task<IGameProfileInfo?> GetProfileInfo(string profileName, IStartupOptions startupOptions, IUser user);
        Task<IGameProfileInfo?> RestoreProfileInfo(string profileName, IStartupOptions startupOptions, IUser user);
        Task PackProfile(IGameProfile baseProfile);
        Task AddFileToWhiteList(IGameProfile profile, IFileInfo file);
        Task RemoveFileFromWhiteList(IGameProfile profile, IFileInfo file);
        Task UpdateProfile(IGameProfile profile, string newProfileName, string newIcon, string newDescription);
        Task<string[]> InstallAuthLib(IGameProfile profile);
        Task<IGameProfileInfo?> GetCacheProfile(IGameProfile baseProfile);
        Task SetCacheProfile(IGameProfileInfo profile);
    }
}
