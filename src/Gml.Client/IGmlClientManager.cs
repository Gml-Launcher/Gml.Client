using System.ComponentModel;
using System.Diagnostics;
using Gml.WebApi.Models.Dtos.Profiles;

namespace Gml.Client;

public interface IGmlClientManager
{
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    Task<IEnumerable<ReadProfileDto>> GetProfiles();
    Task<ProfileInfoReadDto?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto);
    Task DownloadFiles(IEnumerable<LocalFileInfoDto> files, int loadFilesPartCount);
    Task<Process> GetProcess(ProfileCreateInfoDto profileName);
    Task DownloadNotInstalledFiles(ProfileInfoReadDto profileInfo);
}