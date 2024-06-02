using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using DiscordRPC;
using Gml.Client.Models;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Integration;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Profile;
using Gml.Web.Api.Dto.Texture;
using Gml.Web.Api.Dto.User;
using System.Text.Json;

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

    private Dictionary<string, List<ProfileFileWatcher>> _fileWatchers = new();
    private (DiscordRpcClient? Client, DiscordRpcReadDto? ClientInfo)? _discordRpcClient;

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

        return JsonSerializer.Deserialize<ResponseMessage<List<ProfileReadDto>>>(content)
               ?? new ResponseMessage<List<ProfileReadDto>>();
    }

    public async Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto)
    {
        Debug.Write("Get profile info");
        var model = JsonSerializer.Serialize(profileCreateInfoDto);

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/profiles/info", data).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        Debug.Write("Profile loaded");
        return JsonSerializer.Deserialize<ResponseMessage<ProfileReadInfoDto?>>(content);
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
        if (_fileWatchers.TryGetValue(profile.ProfileName, out var watchers))
        {
            foreach (var fileWatcher in watchers)
            {
                fileWatcher.Process = process;
                fileWatcher.AllowFiles = allowFiles;
            }
        }
        else
        {
            var modsWatcher = new ProfileFileWatcher(Path.Combine(profilePath, "mods"), profile.Files, process);
            var assetsWatcher = new ProfileFileWatcher(Path.Combine(profilePath, "assets", "skins"), profile.Files, process, false);

            modsWatcher.FileAdded += (sender, args) => FileAdded?.Invoke(sender, args);
            assetsWatcher.FileAdded += (sender, filePath) => FileAdded?.Invoke(sender, filePath);

            _fileWatchers[profile.ProfileName] =
            [
                modsWatcher,
                assetsWatcher
            ];
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
                var url = JsonSerializer.Deserialize<ResponseMessage<UrlServiceDto>>(content);

                return url?.Data?.Url ?? string.Empty;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        return string.Empty;
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password, string hwid)
    {
        var model = JsonSerializer.Serialize(new BaseUserPassword
        {
            Login = login,
            Password = password
        });

        var authUser = new AuthUser
        {
            Name = login
        };

        var data = new StringContent(model, Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Add("X-HWID", hwid);
        var response = await _httpClient.PostAsync("/api/v1/integrations/auth/signin", data).ConfigureAwait(false);
        _httpClient.DefaultRequestHeaders.Remove("X-HWID");
        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var dto = JsonSerializer.Deserialize<ResponseMessage<AuthUser>>(content);

        if (response.IsSuccessStatusCode && dto != null)
        {
            authUser.Uuid = dto.Data!.Uuid;
            authUser.AccessToken = dto.Data!.AccessToken;
            authUser.Has2Fa = dto.Data!.Has2Fa;
            authUser.ExpiredDate = dto.Data!.ExpiredDate;
            authUser.TextureUrl = dto.Data.TextureUrl;

            return (authUser, string.Empty, Enumerable.Empty<string>());
        }

        return (authUser, dto?.Message ?? string.Empty, dto?.Errors ?? []);
    }

    public async Task DownloadFiles(string installationDirectory, ProfileFileReadDto[] files, int loadFilesPartCount,
        CancellationToken cancellationToken = default)
    {
        _progress = 0;
        _finishedFilesCount = 0;
        _progressFilesCount = files.Length;

        var throttler = new SemaphoreSlim(loadFilesPartCount);

        var tasks = files.Select(file => DownloadFileWithRetry(installationDirectory, file, throttler, cancellationToken));

        // Исполнение всех задач.
        await Task.WhenAll(tasks);
    }

    private async Task DownloadFileWithRetry(string installationDirectory, ProfileFileReadDto file,
        SemaphoreSlim throttler, CancellationToken cancellationToken = default)
    {
        // Try to download file up to 3 times
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await DownloadFile(installationDirectory, file, throttler, cancellationToken);
                return;
            }
            catch
            {
                if (attempt == 3)
                    throw;
            }
        }
    }

    private async Task DownloadFile(string installationDirectory, ProfileFileReadDto file, SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        await throttler.WaitAsync(cancellationToken);

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
                using (var stream = await _httpClient.GetStreamAsync(url))
                {
                    await stream.CopyToAsync(fs, cancellationToken);
                }
            }

            _finishedFilesCount++;
            _progress = Convert.ToInt16(_finishedFilesCount * 100 / _progressFilesCount);
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(_progress, null));
            Debug.WriteLine($"{_finishedFilesCount}/{_progressFilesCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            throttler.Release(); // Возвращаем пройденное разрешение обратно в SemaphoreSlim.
        }
    }

    private Task EnsureDirectoryExists(string localPath)
    {
        var directory = Path.GetDirectoryName(localPath);
        if (directory == null) return Task.CompletedTask;

        var directoryInfo = new DirectoryInfo(directory);
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        return Task.CompletedTask;
    }

    public Task LoadDiscordRpc() => GetDiscordRpcClient();

    public async Task UpdateDiscordRpcState(string state)
    {
        _discordRpcClient ??= await GetDiscordRpcClient();

        if (await GetDiscordClient() is { } discordClient)
        {
            _discordRpcClient?.Client?.SetPresence(new RichPresence
            {
                Timestamps = Timestamps.Now,
                Details = discordClient.Details,
                State = state,
                Assets = new Assets
                {
                    LargeImageKey = discordClient.LargeImageKey,
                    LargeImageText = discordClient.LargeImageText,
                    SmallImageKey = discordClient.SmallImageKey,
                    SmallImageText = discordClient.SmallImageText
                }
            });
        }
    }

    private async Task<(DiscordRpcClient? Client, DiscordRpcReadDto? ClientInfo)> GetDiscordRpcClient()
    {
        if (_discordRpcClient?.Client is null)
        {
            var discordClient = await GetDiscordClient();

            if (discordClient is null || string.IsNullOrEmpty(discordClient.ClientId))
            {
                return (null, null);
            }

            var client = new DiscordRpcClient(discordClient.ClientId);
            client.Initialize();

            Debug.WriteLine($"DiscordRPC is Initialized: {client.IsInitialized}");
            return (client, discordClient);
        }

        return (null, null);
    }

    public async Task<DiscordRpcReadDto?> GetDiscordClient()
    {
        var response = await _httpClient.GetAsync("/api/v1/integrations/discord").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var result = JsonSerializer.Deserialize<ResponseMessage<DiscordRpcReadDto?>>(content);

        return result?.Data;
    }
}
