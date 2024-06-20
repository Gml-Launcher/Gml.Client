using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mime;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Gml.Client.Helpers;
using Gml.Client.Models;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;
using GmlCore.Interfaces.Storage;
using Gml.Client.Extensions;

namespace Gml.Client;

public class GmlClientManager : IGmlClientManager
{
    IObservable<int> IGmlClientManager.ProgressChanged => _progressChanged;

    public string ProjectName { get; }

    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    private readonly string _installationDirectory;
    private readonly ApiProcedures _apiProcedures;
    private readonly SystemIoProcedures _systemProcedures;
    private ISubject<int> _progressChanged = new Subject<int>();
    private SignalRConnect? _launchbackendConnection;
    private readonly string _webSocketAddress;

    public GmlClientManager(string installationDirectory, string gateWay, string projectName, OsType osType)
    {
        _installationDirectory = installationDirectory;

        var hostUri = new Uri(gateWay);

        _systemProcedures = new SystemIoProcedures(installationDirectory, osType);
        _apiProcedures = new ApiProcedures(new HttpClient
        {
            BaseAddress = hostUri
        }, osType);

        _apiProcedures.ProgressChanged.Subscribe(_progressChanged);

        ProjectName = projectName;

        if (hostUri.Scheme == Uri.UriSchemeHttps)
        {
            _webSocketAddress = "wss://" + hostUri.Host + (hostUri.IsDefaultPort ? "" : ":" + hostUri.Port);
        }
        else if (hostUri.Scheme == Uri.UriSchemeHttp)
        {
            _webSocketAddress = "ws://" + hostUri.Host + (hostUri.IsDefaultPort ? "" : ":" + hostUri.Port);
        }
    }

    public Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles()
        => _apiProcedures.GetProfiles();

    public Task LoadDiscordRpc()
        => _apiProcedures.LoadDiscordRpc();

    public Task UpdateDiscordRpcState(string state)
        => _apiProcedures.UpdateDiscordRpcState(state);

    public Task<IVersionFile?> GetActualVersion(OsType osType) => _apiProcedures.GetActualVersion(osType);

    public async Task UpdateCurrentLauncher(
        (IVersionFile? ActualVersion, bool IsActuallVersion) versionInfo,
        OsType osType, string originalFileName)
    {
        var tempFileName = "Launcher.Update";

        if (versionInfo.ActualVersion is null)
        {
            throw new ArgumentException(nameof(versionInfo.ActualVersion));
        }

        _progressChanged.OnNext(0);

        (Stream Stream, long Bytes) content = await _apiProcedures.GetNewLauncher(versionInfo.ActualVersion.Guid);

        var fs = new FileStream(tempFileName, FileMode.OpenOrCreate);
        await content.Stream.CopyToAsync(fs, content.Bytes,
            new Progress<int>(percentage => { _progressChanged.OnNext(percentage); }));

        _progressChanged.OnNext(100);

        FileReplaceAndRestart(osType, tempFileName, originalFileName);
    }

    private void FileReplaceAndRestart(OsType osType, string newFileName, string originalFileName)
    {
        string cmd;
        switch (osType)
        {
            case OsType.Undefined:
                throw new Exception("Undefined OS type is not supported");
            case OsType.Linux:
            case OsType.OsX:
                cmd = $"sh -c \"while [ -e /proc/{Process.GetCurrentProcess().Id} ]; do sleep 1; done; mv {newFileName} {originalFileName}; exec {originalFileName}\"";
                Process.Start(cmd);
                break;
            case OsType.Windows:
                cmd = $"/C for /L %N in () do (tasklist | findstr {Process.GetCurrentProcess().Id} >NUL || (move /Y {newFileName} {originalFileName} & start {originalFileName} & exit) & timeout 1) >NUL";
                var psi = new ProcessStartInfo("CMD.exe", cmd)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                };
                Process.Start(psi);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(osType), osType, null);
        }

        // Singleton application should end to allow script replace its file
        Environment.Exit(0);
    }

    public Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileDto)
        => _apiProcedures.GetProfileInfo(profileDto);

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, OsType osType)
        => _apiProcedures.GetProcess(profileDto, _installationDirectory, osType);

    public async Task DownloadNotInstalledFiles(ProfileReadInfoDto profileInfo,
        CancellationToken cancellationToken = default)
    {
        await _systemProcedures.RemoveFiles(profileInfo);

        var updateFiles = _systemProcedures.FindErroneousFiles(profileInfo, _installationDirectory);
        await _apiProcedures.DownloadFiles(_installationDirectory, updateFiles.ToArray(), 60, cancellationToken);
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password,
        string hwid)
    {
        var user = await _apiProcedures.Auth(login, password, hwid);

        if (user.User?.IsAuth == true)
        {
            if (_launchbackendConnection?.DisposeAsync().AsTask() is {} task)
            {
                await task;
            }

            await OpenServerConnection(user.User);
        }

        return user;
    }

    public async Task OpenServerConnection(IUser user)
    {
        _launchbackendConnection = new SignalRConnect($"{_webSocketAddress}/ws/launcher", user);
        await _launchbackendConnection.BuildAndConnect();
    }

    private async void UpdateInfo(long _)
    {
        if (_launchbackendConnection is not null)
        {
            await _launchbackendConnection!.UpdateInfo();
        }
    }

    public Task ClearFiles(ProfileReadInfoDto profile) => _systemProcedures.RemoveFiles(profile);

    public static Task<string> GetSentryLink(string hostUrl) => ApiProcedures.GetSentryLink(hostUrl);
    void IDisposable.Dispose()
    {
        _launchbackendConnection?.Dispose();
    }
}
