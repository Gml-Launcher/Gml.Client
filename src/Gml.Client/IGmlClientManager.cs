using System.ComponentModel;
using System.Diagnostics;
using Gml.Client.Models;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;
using GmlCore.Interfaces.Storage;

namespace Gml.Client;

public interface IGmlClientManager
{
    IObservable<int> ProgressChanged { get; }
    public string ProjectName { get; }
    Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles();
    Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileDto);
    public Task<Process> GetProcess(ProfileReadInfoDto profileDto);
    Task DownloadNotInstalledFiles(ProfileReadInfoDto profileInfo, CancellationToken cancellationToken);
    Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password, string hwid);
    Task ClearFiles(ProfileReadInfoDto profile);
    Task LoadDiscordRpc();
    Task UpdateDiscordRpcState(string state);
    Task<IVersionFile?> GetActualVersion(OsType osType);
    Task UpdateCurrentLauncher((IVersionFile? ActualVersion, bool IsActuallVersion) versionInfo, OsType osType,
        string originalFileName);
}
