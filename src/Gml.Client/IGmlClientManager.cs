using System.ComponentModel;
using System.Diagnostics;
using Gml.Client.Models;
using Gml.WebApi.Models.Dtos.Profiles;
using Gml.WebApi.Models.Dtos.Response;

namespace Gml.Client;

public interface IGmlClientManager
{
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    Task<ResponseMessage<List<ReadProfileDto>>> GetProfiles();
    Task<ResponseMessage<ProfileInfoReadDto?>?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto);
    Task DownloadFiles(IEnumerable<LocalFileInfoDto> files, int loadFilesPartCount);
    public Task<Process> GetProcess(ProfileInfoReadDto profileDto);
    Task DownloadNotInstalledFiles(ProfileInfoReadDto profileInfo);
    Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password);
}
