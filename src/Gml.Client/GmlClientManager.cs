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
    private readonly HttpClient _skinHttpClient;
    private int _progressFilesCount = 0;
    private int _finishedFilesCount = 0;
    private int _progress;

    public GmlClientManager(string gateWay, string skinGateWay, string installationDirectory)
    {
        _installationDirectory = installationDirectory;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(gateWay),
            Timeout = TimeSpan.FromSeconds(20)

        };

        _skinHttpClient = new HttpClient
        {
            BaseAddress = new Uri(skinGateWay)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"Gml.Launcher-Client-{nameof(GmlClientManager)}/1.0 (OS: {Environment.OSVersion};)");
    }

    public async Task<ResponseMessage<List<ReadProfileDto>>> GetProfiles()
    {
        Console.Write("Load profiles: ");
        var response = await _httpClient.GetAsync("/api/v1/profiles");

        Console.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return new ResponseMessage<List<ReadProfileDto>>();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ResponseMessage<List<ReadProfileDto>>>(content)
               ?? new ResponseMessage<List<ReadProfileDto>>();
    }

    public async Task<ResponseMessage<ProfileInfoReadDto?>?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto)
    {
        var model = JsonConvert.SerializeObject(profileCreateInfoDto);

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/profiles/info", data);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ResponseMessage<ProfileInfoReadDto>>(content);
    }

    public async Task DownloadFiles(IEnumerable<LocalFileInfoDto> files, int loadFilesPartCount = 16)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));

        _progressFilesCount = files.Count();
        _finishedFilesCount = 0;

        var semaphore = new SemaphoreSlim(loadFilesPartCount);

        var downloadTasks = files.Select(fileInfo =>
            DownloadFileAsync($"{_httpClient.BaseAddress.AbsoluteUri}api/v1/file/{fileInfo.Hash}", _httpClient, semaphore,
                string.Join("", _installationDirectory, fileInfo.Directory)));

        await Task.WhenAll(downloadTasks);
    }

    public Task<Process> GetProcess(ProfileInfoReadDto profileDto)
    {
        var profilePath = _installationDirectory + @"\clients\" + profileDto.ProfileName;

        var arguments = profileDto!.Arguments.Replace("{localPath}", profilePath);

        var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = profileDto.JavaPath.Replace("{localPath}", profilePath),
            Arguments = arguments.Replace("{authEndpoint}", $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/authlib/minecraft"),
        };
        //http://localhost:5000/api/v1/integrations/authlib/minecraft
        //http://localhost:5000/api/v1/integrations/authlib/minecraft



        return Task.FromResult(process);
    }

    public Task<IEnumerable<LocalFileInfoDto>> FindErroneousFiles(ProfileInfoReadDto profileInfo)
    {
        var errorFiles = (from downloadingFile in profileInfo.Files
            let localPath = _installationDirectory + downloadingFile.Directory
            where profileInfo.WhiteListFiles.Count <= 0 || profileInfo.WhiteListFiles.All(c => c.Directory != downloadingFile.Directory) ||
                  !File.Exists(localPath)
            where File.Exists(localPath) == false ||
                  SystemHelper.CalculateFileHash(localPath, new SHA256Managed()) != downloadingFile.Hash
            select downloadingFile).ToList();

        return Task.FromResult(errorFiles.AsEnumerable());
    }

    public async Task DownloadNotInstalledFiles(ProfileInfoReadDto profileInfo)
    {
        var updateFiles = await FindErroneousFiles(profileInfo);

        await DownloadFiles(updateFiles, 64);
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password)
    {
        var model = JsonConvert.SerializeObject(new AuthDto
        {
            Login = login,
            Password = password
        });

        var authUser = new AuthUser
        {
            Name = login
        };

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/integrations/auth/signin", data);

        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync();

        var dto = JsonConvert.DeserializeObject<ResponseMessage<AuthUser>>(content);

        if (response.IsSuccessStatusCode && dto != null)
        {
            authUser.Uuid = dto.Data!.Uuid;
            authUser.AccessToken = dto.Data!.AccessToken;
            authUser.Has2Fa = dto.Data!.Has2Fa;
            authUser.ExpiredDate = dto.Data!.ExpiredDate;
            authUser.TextureUrl = dto.Data.TextureUrl;

            return (authUser, string.Empty, Enumerable.Empty<string>());
        }

        return (authUser, dto?.Message ?? string.Empty, dto?.Errors ?? Enumerable.Empty<string>());
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
