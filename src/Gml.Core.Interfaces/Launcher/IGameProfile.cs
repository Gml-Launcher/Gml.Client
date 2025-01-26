using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Mods;
using GmlCore.Interfaces.Procedures;
using GmlCore.Interfaces.Servers;
using GmlCore.Interfaces.System;
using GmlCore.Interfaces.User;

namespace GmlCore.Interfaces.Launcher
{
    public interface IGameProfile : IDisposable
    {
        /// <summary>
        /// Responsible for handling profile-specific operations.
        /// </summary>
        [JsonIgnore] IProfileProcedures ProfileProcedures { get; set; }

        /// <summary>
        /// Responsible for server-specific operations related to the profile.
        /// </summary>
        [JsonIgnore] IProfileServersProcedures ServerProcedures { get; set; }

        /// <summary>
        /// Manages game downloading operations.
        /// </summary>
        [JsonIgnore] IGameDownloaderProcedures GameLoader { get; set; }

        /// <summary>
        /// Name of the game profile.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Indicates if the game profile is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Version of the game.
        /// </summary>
        string GameVersion { get; set; }

        /// <summary>
        /// Version of the game at launch.
        /// </summary>
        string? LaunchVersion { get; set; }

        /// <summary>
        /// Game loader associated with the profile.
        /// </summary>
        GameLoader Loader { get; }

        /// <summary>
        /// Path to the game client.
        /// </summary>
        string ClientPath { get; set; }

        /// <summary>
        /// Base64 encoded icon for the profile.
        /// </summary>
        string IconBase64 { get; set; }

        /// <summary>
        /// Key for the background image.
        /// </summary>
        string BackgroundImageKey { get; set; }

        /// <summary>
        /// Description of the game profile.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// List of files permitted by the profile.
        /// </summary>
        List<IFileInfo>? FileWhiteList { get; set; }

        /// <summary>
        /// List of folders permitted by the profile.
        /// </summary>
        List<IFolderInfo>? FolderWhiteList { get; set; }

        /// <summary>
        /// List of user GUIDs permitted by the profile.
        /// </summary>
        List<string> UserWhiteListGuid { get; set; }

        /// <summary>
        /// List of servers associated with the profile.
        /// </summary>
        List<IProfileServer> Servers { get; }

        /// <summary>
        /// Represents a collection of optional mods that can be selected to enhance or customize the game experience.
        /// </summary>
        List<IMod> OptionalMods { get; }

        /// <summary>
        /// Represents a collection of core modifications or modules applicable to the game profile.
        /// </summary>
        List<IMod> Mods { get; }

        /// <summary>
        /// Date and time when the profile was created.
        /// </summary>
        DateTimeOffset CreateDate { get; }

        /// <summary>
        /// JVM arguments for the game.
        /// </summary>
        string? JvmArguments { get; set; }

        /// <summary>
        /// Game arguments used at runtime.
        /// </summary>
        string? GameArguments { get; set; }

        /// <summary>
        /// Current state of the game profile.
        /// </summary>
        ProfileState State { get; set; }

        /// <summary>
        /// Validates the game profile.
        /// </summary>
        Task<bool> ValidateProfile();

        /// <summary>
        /// Checks if the profile is fully loaded.
        /// </summary>
        Task<bool> CheckIsFullLoaded(IStartupOptions startupOptions);

        /// <summary>
        /// Removes the game profile.
        /// </summary>
        Task Remove();

        /// <summary>
        /// Initiates the download process for the game.
        /// </summary>
        Task DownloadAsync();

        /// <summary>
        /// Creates a process for the game.
        /// </summary>
        Task<Process> CreateProcess(IStartupOptions startupOptions, IUser user);

        /// <summary>
        /// Checks if the game client exists.
        /// </summary>
        Task<bool> CheckClientExists();

        /// <summary>
        /// Checks if the operating system type is loaded.
        /// </summary>
        Task<bool> CheckOsTypeLoaded(IStartupOptions startupOptions);

        /// <summary>
        /// Installs authentication libraries.
        /// </summary>
        Task<string[]> InstallAuthLib();

        /// <summary>
        /// Retrieves cached profile information.
        /// </summary>
        Task<IGameProfileInfo?> GetCacheProfile();

        /// <summary>
        /// Adds a server to the profile.
        /// </summary>
        void AddServer(IProfileServer server);

        /// <summary>
        /// Removes a server from the profile.
        /// </summary>
        void RemoveServer(IProfileServer server);

        /// <summary>
        /// Creates a mods folder for the profile.
        /// </summary>
        Task CreateModsFolder();

        /// <summary>
        /// Retrieves profile files based on operating system details.
        /// </summary>
        Task<ICollection<IFileInfo>> GetProfileFiles(string osName, string osArchitecture);

        /// <summary>
        /// Retrieves all profile files, optionally restoring from cache.
        /// </summary>
        Task<IFileInfo[]> GetAllProfileFiles(bool needRestoreCache);

        /// <summary>
        /// Creates a user session asynchronously.
        /// </summary>
        Task CreateUserSessionAsync(IUser user);

        /// <summary>
        /// Retrieves the mods associated with the game profile.
        /// </summary>
        Task<IEnumerable<IMod>> GetModsAsync();

        /// <summary>
        /// Retrieves the optional mods associated with a game profile.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of optional mods.</returns>
        Task<IEnumerable<IMod>> GetOptionalsModsAsync();

        Task<IMod> AddMod(string fileName, Stream streamData);
        Task<IMod> AddOptionalMod(string fileName, Stream streamData);
        Task<bool> RemoveMod(string modName);
    }
}
