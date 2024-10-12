using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gml.Client.Models;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;
using GmlCore.Interfaces.Storage;

namespace Gml.Client;

public interface IGmlClientManager : IDisposable
{
    IObservable<int> ProgressChanged { get; }
    IObservable<bool> ProfilesChanges { get; }
    public string ProjectName { get; }
    IObservable<int> MaxFileCount { get; }
    IObservable<int> LoadedFilesCount { get; }
    string InstallationDirectory { get; }
    Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles();
    Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileDto);
    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, OsType osType);
    Task DownloadNotInstalledFiles(ProfileReadInfoDto profileInfo, CancellationToken cancellationToken);
    Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password, string hwid);
    Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string accessToken);
    Task ClearFiles(ProfileReadInfoDto profile);
    Task LoadDiscordRpc();
    Task UpdateDiscordRpcState(string state);
    Task<IVersionFile?> GetActualVersion(OsType osType, Architecture osArch);
    Task UpdateCurrentLauncher((IVersionFile? ActualVersion, bool IsActuallVersion) versionInfo, OsType osType,
        string originalFileName);
    Task OpenServerConnection(IUser user);
    void ChangeInstallationFolder(string installationDirectory);
}
