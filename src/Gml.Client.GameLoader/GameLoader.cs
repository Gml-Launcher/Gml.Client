using System.Diagnostics;
using System.Runtime.InteropServices;
using Gml.Client.Exceptions;
using Gml.Client.Interfaces;
using Gml.Dto.Messages;
using Gml.Dto.Profile;
using Gml.Web.Api.Domains.System;
using static System.OperatingSystem;

namespace Gml.Client;

public class GameLoader : IGameLoader
{
    public Task StartGameAsync(ResponseMessage<ProfileReadInfoDto?>? profileInfo, bool isOnline)
    {
        if (profileInfo?.Data is null)
        {
            throw new ProfileNotLoadedException();
        }

        var osType = GetOsType();

        return InitializeAsync(profileInfo.Data!, osType, isOnline, CancellationToken.None);
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

        var process = await Manager.GetProcess(profile, osType, !isOnline);

        // process.OutputDataReceived += (_, e) =>
        // {
        //     if (!string.IsNullOrEmpty(e.Data))
        //     {
        //         Debug.WriteLine(e.Data);
        //         _logHandler.ProcessLogs(e.Data);
        //     }
        // };
        //
        // process.ErrorDataReceived += (sender, e) =>
        // {
        //     if (e.Data is null || string.IsNullOrEmpty(e.Data))
        //     {
        //         return;
        //     }
        //
        //     Debug.WriteLine(e.Data);
        //
        //     _logHandler.ProcessLogs(e.Data);
        // };

        // UpdateProgress(
        //     LocalizationService.GetString(ResourceKeysDictionary.Launching),
        //     LocalizationService.GetString(ResourceKeysDictionary.PreparingLaunch),
        //     true);
        //
        // return process;

    }

    public IGmlClientManager Manager { get; set; }
}
