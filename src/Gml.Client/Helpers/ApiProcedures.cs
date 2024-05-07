using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using DiscordRPC;
using Gml.Client.Models;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;
using Gml.Web.Api.Dto.Texture;
using Gml.Web.Api.Dto.User;
using Newtonsoft.Json;

namespace Gml.Client.Helpers;

public class ApiProcedures
{
    private readonly HttpClient _httpClient;
    private readonly OsType _osType;

    private int _progressFilesCount;
    private int _finishedFilesCount;
    private int _progress;

    internal event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    internal event EventHandler<string>? FileAdded;

    private Dictionary<string, ProfileFileWatcher> _fileWatchers = new();
    private DiscordRpcClient? _discordRpcClient;

    public ApiProcedures(HttpClient httpClient, OsType osType)
    {
        _httpClient = httpClient;
        _osType = osType;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"Gml.Launcher-Client-{nameof(GmlClientManager)}/1.0 (OS: {Environment.OSVersion};)");
    }

    public async Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles()
    {
        Debug.Write("Load profiles: ");
        var response = await _httpClient.GetAsync("/api/v1/profiles").ConfigureAwait(false);

        Debug.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return new ResponseMessage<List<ProfileReadDto>>();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonConvert.DeserializeObject<ResponseMessage<List<ProfileReadDto>>>(content)
               ?? new ResponseMessage<List<ProfileReadDto>>();
    }

    public async Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto)
    {
        Debug.Write("Get profile info");
        var model = JsonConvert.SerializeObject(profileCreateInfoDto);

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/profiles/info", data).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        Debug.Write("Profile loaded");
        return JsonConvert.DeserializeObject<ResponseMessage<ProfileReadInfoDto?>>(content);
    }

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, string installationDirectory)
    {
        var process = GetStartProcess(profileDto, installationDirectory);

        return Task.FromResult(process);
    }

    private List<ProfileFileReadDto> GetAllowFiles(ProfileReadInfoDto profileDto)
    {
        return profileDto.Files.Where(c => c.Directory.Contains(@"\mods\") || c.Directory.Contains("/mods/")).ToList();
    }

    private Process GetStartProcess(ProfileReadInfoDto profileDto, string installationDirectory)
    {
        var profilePath = installationDirectory + @"\clients\" + profileDto.ProfileName;

        var parameters = new Dictionary<string, string>
        {
            { "{localPath}", profilePath },
            { "{authEndpoint}", $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/authlib/minecraft" },
        };

        foreach (var parameter in parameters)
        {
            profileDto.Arguments = profileDto.Arguments.Replace(parameter.Key, parameter.Value);
        }

        var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = profileDto.JavaPath.Replace("{localPath}", profilePath),
            Arguments = profileDto.Arguments,
            WorkingDirectory = profilePath
        };

        InitializeFileWatchers(process, profileDto, GetAllowFiles(profileDto), profilePath);
        return process;
    }

    private void InitializeFileWatchers(Process process,
        ProfileReadInfoDto profile,
        List<ProfileFileReadDto> allowFiles, string profilePath)
    {
        if (_fileWatchers.TryGetValue(profile.ProfileName, out var fileWatcher))
        {
            fileWatcher.Process = process;
            fileWatcher.AllowFiles = allowFiles;
        }
        else
        {
            _fileWatchers[profile.ProfileName] = new ProfileFileWatcher($"{profilePath}", profile.Files, process);
            _fileWatchers[profile.ProfileName].FileAdded += (sender, args) => FileAdded?.Invoke(sender, args);
        }
    }

    public static async Task<string> GetSentryLink(string hostUrl)
    {
        try
        {
            using var client = new HttpClient();

            var request = await client
                .GetAsync($"{hostUrl}/api/v1/integrations/sentry/dsn")
                .ConfigureAwait(false);

            if (request.IsSuccessStatusCode)
            {
                var content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                var url = JsonConvert.DeserializeObject<ResponseMessage<UrlServiceDto>>(content);

                return url?.Data?.Url ?? string.Empty;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        return string.Empty;
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password)
    {
        var model = JsonConvert.SerializeObject(new BaseUserPassword
        {
            Login = login,
            Password = password
        });

        var authUser = new AuthUser
        {
            Name = login
        };

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/integrations/auth/signin", data).ConfigureAwait(false);

        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

    public async Task DownloadFiles(string installationDirectory, ProfileFileReadDto[] files, int loadFilesPartCount)
{
    _progress = 0;
    _finishedFilesCount = 0;
    _progressFilesCount = files.Length;

    var throttler = new SemaphoreSlim(loadFilesPartCount);

    var tasks = files.Select(file => DownloadFileWithRetry(installationDirectory, file, throttler));

    // Исполнение всех задач.
    await Task.WhenAll(tasks);
}

private async Task DownloadFileWithRetry(string installationDirectory, ProfileFileReadDto file, SemaphoreSlim throttler)
{
    // Try to download file up to 3 times
    for (int attempt = 1; attempt <= 3; attempt++)
    {
        try
        {
            await DownloadFile(installationDirectory, file, throttler);
            return;
        }
        catch
        {
            if (attempt == 3) // if last try throws, don't swallow exception
                throw;
            // add delay before next try
            await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
        }
    }
}

private async Task DownloadFile(string installationDirectory, ProfileFileReadDto file, SemaphoreSlim throttler)
{
    await throttler.WaitAsync();

    try
    {
        if (_osType == OsType.Windows)
        {
            file.Directory = file.Directory.Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);
        }

        string localPath = Path.Combine(installationDirectory, file.Directory);
        await EnsureDirectoryExists(localPath);

        var url = $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/file/{file.Hash}";

        using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
        {
            var stream = await _httpClient.GetStreamAsync(url);

            await stream.CopyToAsync(fs);
        }

        _finishedFilesCount++;
        _progress = Convert.ToInt16(_finishedFilesCount * 100 / _progressFilesCount);
        ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(_progress, null));
    }
    finally
    {
        throttler.Release(); // Возвращаем пройденное разрешение обратно в SemaphoreSlim.
    }
}

private async Task EnsureDirectoryExists(string localPath)
{
    var directory = Path.GetDirectoryName(localPath);
    if (directory == null) return;

    var directoryInfo = new DirectoryInfo(directory);
    if (!directoryInfo.Exists)
    {
        directoryInfo.Create();
    }
}

    // public Task<IEnumerable<ProfileFileReadDto>> FindErrorFiles(
    //     ProfileReadInfoDto profileInfo,
    //     string installationDirectory)
    // {
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    // }
    public async Task LoadDiscordRpc()
    {
        _discordRpcClient ??= await GetDiscordRpcClient();

        _discordRpcClient?.SetPresence(new RichPresence
        {
            Details = "Gml.Launcher",
            State = "Сидит в лаунчере",
            Assets = new Assets
            {
                LargeImageKey = "logo",
                LargeImageText = "Gml.Launcher",
                SmallImageKey = "logo",
                SmallImageText = "Sashok or Gravity, maybe you'll go fuck yourself"
            }
        });
    }
    public async Task UpdateDiscordRpcState(string state)
    {
        _discordRpcClient ??= await GetDiscordRpcClient();

        if (_discordRpcClient?.CurrentPresence is { } discordPresence)
        {
            discordPresence.State = state;
            _discordRpcClient.SetPresence(new RichPresence
            {
                Timestamps = Timestamps.Now,
                Details = "Gml.Launcher",
                State = state,
                Assets = new Assets
                {
                    LargeImageKey = "logo",
                    LargeImageText = "Gml.Launcher",
                    SmallImageKey = "logo",
                    SmallImageText = "Sashok or Gravity, maybe you'll go fuck yourself"
                }
            });
        }

    }

    private async Task<DiscordRpcClient?> GetDiscordRpcClient()
    {
        if (_discordRpcClient is null)
        {

            var clientId = await GetDiscordClient("");

            if (string.IsNullOrEmpty(clientId))
            {
                return null;
            }

            _discordRpcClient = new DiscordRpcClient(clientId);
            _discordRpcClient.Initialize();
            Debug.WriteLine($"DiscordRPC is Initialized: {_discordRpcClient.IsInitialized}");
        }

        return _discordRpcClient;
    }

    public static Task<string?> GetDiscordClient(string hostUrl)
    {

        return Task.FromResult("1205450995820265522");
    }
}

