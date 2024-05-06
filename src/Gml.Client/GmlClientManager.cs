using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using Gml.Client.Helpers;
using Gml.Client.Models;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;

namespace Gml.Client;

public class GmlClientManager : IGmlClientManager
{
    public string ProjectName { get; }

    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    private readonly string _installationDirectory;
    private readonly ApiProcedures _apiProcedures;
    private readonly SystemIoProcedures _systemProcedures;

    public GmlClientManager(string installationDirectory, string gateWay, string projectName, OsType osType)
    {
        _installationDirectory = installationDirectory;

        _systemProcedures = new SystemIoProcedures(installationDirectory, osType);
        _apiProcedures = new ApiProcedures(new HttpClient
        {
            BaseAddress = new Uri(gateWay)
        }, osType);

        _apiProcedures.ProgressChanged += (sender, args) => ProgressChanged?.Invoke(sender, args);

        ProjectName = projectName;
    }

    public Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles()
        => _apiProcedures.GetProfiles();

    public Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileDto)
        => _apiProcedures.GetProfileInfo(profileDto);

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto)
        => _apiProcedures.GetProcess(profileDto, _installationDirectory);

    public async Task DownloadNotInstalledFiles(ProfileReadInfoDto profileInfo)
    {
        await _systemProcedures.RemoveFiles(profileInfo);

        var updateFiles = await _systemProcedures.FindErroneousFilesAsync(profileInfo, _installationDirectory);
        await _apiProcedures.DownloadFiles(_installationDirectory, updateFiles, 16);
    }

    public Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password)
        => _apiProcedures.Auth(login, password);

    public static Task<string> GetSentryLink(string hostUrl) => ApiProcedures.GetSentryLink(hostUrl);
}
