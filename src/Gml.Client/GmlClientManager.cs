using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Gml.Client.Helpers;
using Gml.Client.Models;
using Gml.WebApi.Models.Dtos.Profiles;
using Gml.WebApi.Models.Dtos.Response;
using Gml.WebApi.Models.Dtos.Users;
using Newtonsoft.Json;

namespace Gml.Client;

public class GmlClientManager : IGmlClientManager
{
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    private readonly string _installationDirectory;
    private readonly HttpClient _httpClient;
    private int _progressFilesCount = 0;
    private int _finishedFilesCount = 0;
    private int _progress;

    public GmlClientManager(string gateWay, string installationDirectory)
    {
        _installationDirectory = installationDirectory;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(gateWay)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Gml.Launcher-Client-{nameof(GmlClientManager)}/1.0 (OS: {Environment.OSVersion};)");
    }

    public async Task<IEnumerable<ReadProfileDto>> GetProfiles()
    {
        Console.Write("Load profiles: ");
        var response = await _httpClient.GetAsync("/api/profiles");

        Console.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<ReadProfileDto>();

        var content = await response.Content.ReadAsStringAsync();


        return JsonConvert.DeserializeObject<List<ReadProfileDto>>(content)
               ?? Enumerable.Empty<ReadProfileDto>();
    }

    public async Task<ProfileInfoReadDto?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto)
    {
        var model = JsonConvert.SerializeObject(profileCreateInfoDto);

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/profiles/info", data);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ProfileInfoReadDto>(content);
    }

    public async Task DownloadFiles(IEnumerable<LocalFileInfoDto> files, int loadFilesPartCount = 16)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));

        _progressFilesCount = files.Count();
        _finishedFilesCount = 0;

        var semaphore = new SemaphoreSlim(loadFilesPartCount);

        var downloadTasks = files.Select(fileInfo =>
            DownloadFileAsync($"{_httpClient.BaseAddress.AbsoluteUri}api/file/{fileInfo.Hash}", _httpClient, semaphore,
                string.Join("", _installationDirectory, fileInfo.Directory)));

        await Task.WhenAll(downloadTasks);
    }

    public Task<Process> GetProcess(ProfileInfoReadDto profileDto)
    {
        var profilePath = _installationDirectory + @"\clients\" + profileDto.ProfileName;

        var arguments = profileDto!.Arguments.Replace("{localPath}",profilePath);

        var process = new Process();

        process.StartInfo = new ProcessStartInfo()
        {
            FileName = profileDto.JavaPath.Replace("{localPath}",profilePath).Replace("/", "\\"),
            Arguments = arguments.Replace("/", "\\"),
        };

        return Task.FromResult(process);
    }

    public Task<IEnumerable<LocalFileInfoDto>> FindErroneousFiles(ProfileInfoReadDto profileInfo)
    {
        var errorFiles = new List<LocalFileInfoDto>();

        foreach (var downloadingFile in profileInfo.Files)
        {
            var localPath = _installationDirectory + downloadingFile.Directory;

            if (profileInfo.WhiteListFiles.Count > 0 && profileInfo.WhiteListFiles.Any(c => c.Directory == downloadingFile.Directory) && File.Exists(localPath))
                continue;


            if (File.Exists(localPath) == false || SystemHelper.CalculateFileHash(localPath, new SHA256Managed()) != downloadingFile.Hash)
            {
                errorFiles.Add(downloadingFile);
            }
        }

        return Task.FromResult(errorFiles.AsEnumerable());
    }

    public async Task DownloadNotInstalledFiles(ProfileInfoReadDto profileInfo)
    {
        var updateFiles = await FindErroneousFiles(profileInfo);

        await DownloadFiles(updateFiles, 64);
    }

    public async Task<(IUser, string)> Auth(string login, string password)
    {
        var model = JsonConvert.SerializeObject(new AuthDto
        {
            Login = login,
            Password = password
        });

        var authUser = new AuthUser
        {
            Name = login,
        };

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/auth", data);

        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync();

        var dto = JsonConvert.DeserializeObject<ResponseMessage<AuthUser>>(content);

        if (response.IsSuccessStatusCode && dto != null)
        {
            authUser.Uuid = dto.Data!.Uuid;
            authUser.AccessToken = dto.Data!.AccessToken;
            authUser.Has2Fa = dto.Data!.Has2Fa;
            authUser.ExpiredDate = dto.Data!.ExpiredDate;

            return (authUser, string.Empty);
        }


        return (authUser, dto?.Message ?? string.Empty);
    }

    private async Task DownloadFileAsync(string url, HttpClient httpClient, SemaphoreSlim semaphore,
        string fileName)
    {
        try
        {
            await semaphore.WaitAsync();

            var response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var fileInfo = new FileInfo(fileName);

            if (!fileInfo.Directory!.Exists)
                fileInfo.Directory.Create();

            await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
                fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await contentStream.CopyToAsync(fileStream);
            _finishedFilesCount++;
            _progress = Convert.ToInt16(_finishedFilesCount * 100 / _progressFilesCount);
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(_progress, null));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке файла {url}: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }
}
