using System.ComponentModel;
using System.Diagnostics;
using Gml.Client.Models;
using Gml.WebApi.Models.Dtos.Profiles;

namespace Gml.Client;

public interface IGmlClientManager
{
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    Task<IEnumerable<ReadProfileDto>> GetProfiles();
    Task<ProfileInfoReadDto?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto);
    Task DownloadFiles(IEnumerable<LocalFileInfoDto> files, int loadFilesPartCount);
    public Task<Process> GetProcess(ProfileInfoReadDto profileDto);
    Task DownloadNotInstalledFiles(ProfileInfoReadDto profileInfo);
    Task<(IUser, string)> Auth(string login, string password);
}
