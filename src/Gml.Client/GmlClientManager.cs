using Gml.Client.Extensions;
using Gml.Client.Helpers;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Mods;
using Gml.Web.Api.Dto.News;
using Gml.Web.Api.Dto.Profile;
using Gml.Web.Api.Dto.Servers;
using GmlCore.Interfaces.News;
using GmlCore.Interfaces.Storage;
using GmlCore.Interfaces.User;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using IUser = Gml.Client.Models.IUser;

namespace Gml.Client;

public class GmlClientManager : IGmlClientManager
{
    private readonly ApiProcedures _apiProcedures;
    private readonly ISubject<int> _loadedFilesCount = new Subject<int>();
    private readonly ISubject<int> _maxFileCount = new Subject<int>();
    private readonly OsType _osType;
    private readonly ISubject<bool> _profilesChanged = new Subject<bool>();
    private readonly ISubject<int> _progressChanged = new Subject<int>();
    private readonly string _webSocketAddress;
    private readonly string _offlineProfilesDirectory;

    private SignalRConnect? _launchBackendConnection;
    private IDisposable? _profilesChangedEvent;
    private SystemIoProcedures _systemProcedures;

    public GmlClientManager(string installationDirectory, string gateWay, string projectName, OsType osType)
    {
        InstallationDirectory = installationDirectory;

        var hostUri = new Uri(gateWay);

        _osType = osType;
        _systemProcedures = new SystemIoProcedures(installationDirectory, osType);
        _offlineProfilesDirectory = Path.Combine(InstallationDirectory, "offline-mode");
        _apiProcedures = new ApiProcedures(new HttpClient
        {
            BaseAddress = hostUri
        }, osType);

        _apiProcedures.ProgressChanged.Subscribe(_progressChanged);
        _apiProcedures.LoadedFilesCount.Subscribe(_loadedFilesCount);
        _apiProcedures.MaxFileCount.Subscribe(_maxFileCount);

        ProjectName = projectName;

        if (hostUri.Scheme == Uri.UriSchemeHttps)
            _webSocketAddress = "wss://" + hostUri.Host + (hostUri.IsDefaultPort ? "" : ":" + hostUri.Port);
        else if (hostUri.Scheme == Uri.UriSchemeHttp)
            _webSocketAddress = "ws://" + hostUri.Host + (hostUri.IsDefaultPort ? "" : ":" + hostUri.Port);
    }

    IObservable<int> IGmlClientManager.ProgressChanged => _progressChanged;
    public IObservable<bool> ProfilesChanges => _profilesChanged;
    public IObservable<int> MaxFileCount => _maxFileCount;
    public IObservable<int> LoadedFilesCount => _loadedFilesCount;

    public string ProjectName { get; }
    public string InstallationDirectory { get; private set; }

    public bool SkipUpdate { get; set; }

    [Obsolete("Use method with accessToken")]
    public Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles()
    {
        return _apiProcedures.GetProfiles();
    }

    public Task<ResponseMessage<List<ModsDetailsInfoDto>>> GetOptionalModsInfo(string accessToken)
    {
        return _apiProcedures.GetOptionalModsInfo(accessToken);
    }

    public Task<ResponseMessage<List<NewsReadDto>>> GetNews()
    {
        return _apiProcedures.GetNews();
    }

    public Task<ResponseMessage<List<ModReadDto>>> GetOptionalMods(string profileName, string accessToken)
    {
        return _apiProcedures.GetOptionalMods(profileName, accessToken);
    }

    public bool ToggleOptionalMod(string path, bool isEnebled)
    {
        try
        {
            var newFileName = _apiProcedures.ToggleOptionalMod(path, isEnebled);
            File.Move(path, newFileName);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles(string accessToken)
    {
        return _apiProcedures.GetProfiles(accessToken, _offlineProfilesDirectory);
    }

    public async Task<ResponseMessage<List<ProfileReadDto>>> GetOfflineProfiles()
    {
#if DEBUG
        Debug.WriteLine("Calling GetProfilesOffline()");
#endif
        try
        {
            string? content = await _apiProcedures.ReadJsonResponse(_offlineProfilesDirectory, "profiles");

            if (content == null) { return new ResponseMessage<List<ProfileReadDto>>(); }

            var result = JsonConvert.DeserializeObject<ResponseMessage<List<ProfileReadDto>>>(content);

            if (result?.Data != null)
            {
                foreach (var profile in result.Data)
                {
                    profile.Background = null;
                    profile.State = GmlCore.Interfaces.Enums.ProfileState.Offline;
                    profile.Servers = new List<ServerReadDto>();
                }
            }

            return result ?? new ResponseMessage<List<ProfileReadDto>>();
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Error reading or deserializing json: {ex.Message}");
            SentrySdk.CaptureException(ex);
#endif
            return new ResponseMessage<List<ProfileReadDto>>();
        }
    }

    public Task LoadDiscordRpc()
    {
        return _apiProcedures.LoadDiscordRpc();
    }

    public Task UpdateDiscordRpcState(string state)
    {
        return _apiProcedures.UpdateDiscordRpcState(state);
    }

    public Task<IVersionFile?> GetActualVersion(OsType osType, Architecture osArch)
    {
        return _apiProcedures.GetActualVersion(osType, osArch);
    }

    public async Task UpdateCurrentLauncher(
        (IVersionFile? ActualVersion, bool IsActuallVersion) versionInfo,
        OsType osType, string originalFileName)
    {
        var version = versionInfo.ActualVersion?.Version ?? "1.0.0.0";
        var fileName = $"{Path.GetFileName(originalFileName)}-{version}{Path.GetExtension(originalFileName)}";

        var tempFile = new FileInfo(Path.Combine(Path.GetTempPath(), version, fileName));

        if (tempFile.Exists)
        {
            LauncherUpdater.Start(osType, tempFile.FullName, true);
            return;
        }

        if (!tempFile.Directory!.Exists) tempFile.Directory.Create();

        if (versionInfo.ActualVersion is null) throw new ArgumentException(nameof(versionInfo.ActualVersion));

        _progressChanged.OnNext(0);

        var content = await _apiProcedures.GetNewLauncher(versionInfo.ActualVersion.Guid);

        var fs = new FileStream(tempFile.FullName, FileMode.OpenOrCreate);
        await content.Stream.CopyToAsync(fs, content.Bytes,
            new Progress<int>(percentage => { _progressChanged.OnNext(percentage); }));

        _progressChanged.OnNext(100);
        fs.Close();
        LauncherUpdater.FileReplaceAndRestart(osType, tempFile.FullName, originalFileName);
    }

    public Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileDto)
    {
        return _apiProcedures.GetProfileInfo(profileDto, Path.Combine(_offlineProfilesDirectory, $"{profileDto.ProfileName}"));
    }

    public async Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfoOffline(ProfileCreateInfoDto profileDto)
    {
#if DEBUG
        Debug.WriteLine("Calling GetProfileInfoOffline()");
#endif
        try
        {
            string? content = await _apiProcedures.ReadJsonResponse(Path.Combine(_offlineProfilesDirectory, $"{profileDto.ProfileName}"), "profileInfo");

            if (content == null) { return new ResponseMessage<ProfileReadInfoDto?>(); }

            var dto = JsonConvert.DeserializeObject<ResponseMessage<ProfileReadInfoDto?>>(content);

            if (dto?.Data != null)
            {
                dto.Data.Background = null;
                dto.Data.State = GmlCore.Interfaces.Enums.ProfileState.Offline;
            }

            return dto;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Error reading or deserializing json: {ex.Message}");
            SentrySdk.CaptureException(ex);
#endif
            return new ResponseMessage<ProfileReadInfoDto?>();
        }
    }

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, OsType osType, bool isOffline = false)
    {
        return _apiProcedures.GetProcess(profileDto, InstallationDirectory, osType, isOffline);
    }

    public async Task DownloadNotInstalledFiles(ProfileReadInfoDto profileInfo,
        CancellationToken cancellationToken = default)
    {
        var validateResult = await _systemProcedures.ValidateFilesAsync(profileInfo, InstallationDirectory);

        await _systemProcedures.RemoveFiles(InstallationDirectory, validateResult.ToDelete);
        await _apiProcedures.DownloadFiles(InstallationDirectory, validateResult.ToUpdate, 60, cancellationToken);
    }

    private Task ValidateFilesBeforeInstall(ProfileReadInfoDto profileInfo)
    {
        var updateFiles = _systemProcedures.FindErroneousFiles(profileInfo, InstallationDirectory);

        if (updateFiles.Any())
        {
            SentrySdk.CaptureException(new FileLoadException($"Files not found: {string.Join(Environment.NewLine, updateFiles.Select(c => c.Directory))}"));
        }

        return Task.CompletedTask;
    }

    public async Task DownloadFiles(ProfileFileReadDto[] profileInfo,
        CancellationToken cancellationToken = default)
    {
        await _apiProcedures.DownloadFiles(InstallationDirectory, profileInfo.ToArray(), 60, cancellationToken);
    }

    public Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password,
        string hwid)
    {
        return AuthWith2Fa(login, password, hwid, string.Empty);
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> AuthWith2Fa(string login, string password,
        string hwid, string twoFactorCode)
    {
        var user = await _apiProcedures.AuthWith2Fa(login, password, hwid, twoFactorCode);

        if (user.User?.IsAuth == true)
        {
            if (_launchBackendConnection?.DisposeAsync().AsTask() is { } task) await task;
            await OpenServerConnection(user.User);
        }

        return user;
    }

    public Task<IPlayerTexture?> GetTexturesByName(string userName)
    {
        return _apiProcedures.GetTexturesByName(userName);
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string accessToken)
    {
        var user = await _apiProcedures.Auth(accessToken);

        if (user.User?.IsAuth != true)
            return user;

        if (_launchBackendConnection?.DisposeAsync().AsTask() is { } task) await task;

        await OpenServerConnection(user.User);

        return user;
    }

    public async Task OpenServerConnection(IUser user)
    {
        _profilesChangedEvent?.Dispose();
        if (_launchBackendConnection is not null)
        {
            await _launchBackendConnection.DisposeAsync();
            _profilesChangedEvent?.Dispose();
            _profilesChangedEvent = null;
        }

        _launchBackendConnection = new SignalRConnect($"{_webSocketAddress}/ws/launcher", user);
        _profilesChangedEvent ??= _launchBackendConnection.ProfilesChanges.Subscribe(_profilesChanged);
        await _launchBackendConnection.BuildAndConnect();
    }

    public void ChangeInstallationFolder(string installationDirectory)
    {
        InstallationDirectory = installationDirectory;
        _systemProcedures = new SystemIoProcedures(installationDirectory, _osType);
    }

    public Task ClearFiles(ProfileReadInfoDto profile)
    {
        return _systemProcedures.RemoveFiles(profile);
    }

    void IDisposable.Dispose()
    {
        _launchBackendConnection?.Dispose();
        _profilesChangedEvent?.Dispose();
    }

    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    [DllImport("libc")]
    private static extern int Kill(int pid, int sig);

    private async void UpdateInfo(long _)
    {
        if (_launchBackendConnection is not null) await _launchBackendConnection!.UpdateInfo();
    }

    public static Task<string> GetSentryLink(string hostUrl)
    {
        return ApiProcedures.GetSentryLink(hostUrl);
    }

    public static async Task<string> GetSentryLinkAsync(string hostUrl)
    {
        return await ApiProcedures.GetSentryLink(hostUrl);
    }

    public static async Task<bool> CheckAPI(string hostUrl)
    {
        return await ApiProcedures.CheckBackend(hostUrl);
    }
}
