using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gml.Client.Exceptions;
using Gml.Client.Interfaces;
using Gml.Dto.Messages;
using Gml.Dto.Profile;
using Gml.Web.Api.Domains.System;
using static System.OperatingSystem;

namespace Gml.Client;

public class GameLoader : IGameLoader
{
    private ISubject<Process> _gameLaunched = new Subject<Process>();
    private Process? _process;
    public IObservable<Process> GameLaunched => _gameLaunched.AsObservable();

    public Task StartGameAsync(ResponseMessage<ProfileReadInfoDto?>? profileInfo, bool isOnline)
    {
        if (profileInfo?.Data is null)
        {
            throw new ProfileNotLoadedException();
        }

        var osType = GetOsType();

        return InitializeAsync(profileInfo.Data!, osType, isOnline, CancellationToken.None);
    }

    public void ForceStop()
    {
        try
        {
            _process?.Kill();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private OsType GetOsType()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OsType.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OsType.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OsType.OsX;

        throw new PlatformNotSupportedException();
    }

    protected virtual async Task InitializeAsync(ProfileReadInfoDto profile,
        OsType osType,
        bool isOnline, CancellationToken cancellationToken)
    {
        if (isOnline)
        {
            await Manager.DownloadNotInstalledFiles(profile, cancellationToken);
        }

        _process = await Manager.GetProcess(profile, osType, !isOnline);

        _process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Debug.WriteLine(e.Data);
                // _logHandler.ProcessLogs(e.Data);
            }
        };

        _process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data is null || string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            Debug.WriteLine(e.Data);

            // _logHandler.ProcessLogs(e.Data);
        };

        // UpdateProgress(
        //     LocalizationService.GetString(ResourceKeysDictionary.Launching),
        //     LocalizationService.GetString(ResourceKeysDictionary.PreparingLaunch),
        //     true);
        //
        // return process;
        _process.Start();

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        _gameLaunched.OnNext(_process);

#if NET8_0
        await _process.WaitForExitAsync(cancellationToken);
#endif
        _process.WaitForExit();
    }

    public IGmlClientManager Manager { get; set; }
}
