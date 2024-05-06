using System.ComponentModel;
using System.Diagnostics;
using Gml.Client.Models;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;

namespace Gml.Client;

public interface IGmlClientManager
{
    public string ProjectName { get; }
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles();
    Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileDto);
    public Task<Process> GetProcess(ProfileReadInfoDto profileDto);
    Task DownloadNotInstalledFiles(ProfileReadInfoDto profileInfo);
    Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password);
    Task ClearFiles(ProfileReadInfoDto profile);
}
