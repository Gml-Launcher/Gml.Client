using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using DiscordRPC;
using Gml.Client.Models;
using Gml.Web.Api.Domains.Launcher;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Integration;
using Gml.Web.Api.Dto.Messages;
using Gml.Web.Api.Dto.Player;
using Gml.Web.Api.Dto.Profile;
using Gml.Web.Api.Dto.Texture;
using Gml.Web.Api.Dto.User;
using GmlCore.Interfaces.Storage;
using Newtonsoft.Json;
using static System.OperatingSystem;

namespace Gml.Client.Helpers;

public class ApiProcedures
{
    private readonly HttpClient _httpClient;
    private readonly OsType _osType;

    private int _progressFilesCount;
    private int _finishedFilesCount;
    private int _progress;

    private ISubject<int> _progressChanged = new Subject<int>();
    private ISubject<int> _maxFileCount = new Subject<int>();
    private ISubject<int> _loadedFilesCount = new Subject<int>();
    internal event EventHandler<string>? FileAdded;

    private Dictionary<string, List<ProfileFileWatcher>> _fileWatchers = new();
    private (DiscordRpcClient? Client, DiscordRpcReadDto? ClientInfo)? _discordRpcClient;
    public IObservable<int> ProgressChanged => _progressChanged;
    public IObservable<int> MaxFileCount => _maxFileCount;
    public IObservable<int> LoadedFilesCount => _loadedFilesCount;
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

        var dto = JsonConvert.DeserializeObject<ResponseMessage<ProfileReadInfoDto?>>(content);

        return dto;
    }

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, string installationDirectory, OsType osType)
    {
        var process = GetStartProcess(profileDto, installationDirectory, osType);

        return Task.FromResult(process);
    }

    private List<ProfileFileReadDto> GetAllowFiles(ProfileReadInfoDto profileDto)
    {
        return profileDto.Files.Where(c => c.Directory.Contains(@"\mods\") || c.Directory.Contains("/mods/")).ToList();
    }

    private Process GetStartProcess(ProfileReadInfoDto profileDto, string installationDirectory, OsType osType)
    {
        // var profilePath = installationDirectory + @"\clients\" + profileDto.ProfileName;
        var profilePath = Path.Combine(installationDirectory, "clients", profileDto.ProfileName);

        var parameters = new Dictionary<string, string>
        {
            { "{localPath}", installationDirectory },
            { "{authEndpoint}", $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/authlib/minecraft" },
        };

        foreach (var parameter in parameters)
        {
            profileDto.Arguments = profileDto.Arguments.Replace(parameter.Key, parameter.Value);
        }

        var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = profileDto.JavaPath.Replace("{localPath}", installationDirectory),
            Arguments = profileDto.Arguments,
            WorkingDirectory = profilePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };


        if (!File.Exists(process.StartInfo.FileName))
        {
            throw new FileNotFoundException("Java file not found", process.StartInfo.FileName);
        }

        ChangeProcessRules(process.StartInfo.FileName, osType);
        InitializeFileWatchers(process, profileDto, GetAllowFiles(profileDto), profilePath, installationDirectory);
        return process;
    }

    private void ChangeProcessRules(string startInfoFileName, OsType profileDtoOsType)
    {
        switch (profileDtoOsType)
        {
            case OsType.Undefined:
                break;
            case OsType.Linux:
                var chmodStartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x {startInfoFileName}\""
                };
                Process.Start(chmodStartInfo);
                break;
            case OsType.OsX:
                break;
            case OsType.Windows:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(profileDtoOsType), profileDtoOsType, null);
        }
    }

    private void InitializeFileWatchers(Process process,
        ProfileReadInfoDto profile,
        List<ProfileFileReadDto> allowFiles,
        string profilePath, string installationDirectory)
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
            // var assetsWatcher = new ProfileFileWatcher(
            //     Path.Combine(installationDirectory, "assets", "skins"), profile.Files, process, false);

            modsWatcher.FileAdded += (sender, args) => FileAdded?.Invoke(sender, args);
            // assetsWatcher.FileAdded += (sender, filePath) => FileAdded?.Invoke(sender, filePath);

            _fileWatchers[profile.ProfileName] =
            [
                modsWatcher,
                // assetsWatcher
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

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password,
        string hwid)
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
        _httpClient.DefaultRequestHeaders.Add("X-HWID", hwid);
        var response = await _httpClient.PostAsync("/api/v1/integrations/auth/signin", data).ConfigureAwait(false);
        _httpClient.DefaultRequestHeaders.Remove("X-HWID");
        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var dto = JsonConvert.DeserializeObject<ResponseMessage<PlayerReadDto>>(content);

        if (response.IsSuccessStatusCode && dto != null)
        {
            authUser.Uuid = dto.Data!.Uuid;
            authUser.AccessToken = dto.Data!.AccessToken;
            authUser.Has2Fa = false; //dto.Data!.Has2Fa;
            authUser.ExpiredDate = dto.Data!.ExpiredDate;
            authUser.TextureUrl = dto.Data.TextureSkinUrl;

            return (authUser, string.Empty, Enumerable.Empty<string>());
        }

        return (authUser, dto?.Message ?? string.Empty, dto?.Errors ?? []);
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string accessToken)
    {
        var model = JsonConvert.SerializeObject(new BaseUserPassword
        {
            AccessToken = accessToken,
        });

        var authUser = new AuthUser();

        var data = new StringContent(model, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/v1/integrations/auth/checkToken", data).ConfigureAwait(false);
        _httpClient.DefaultRequestHeaders.Remove("X-HWID");
        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var dto = JsonConvert.DeserializeObject<ResponseMessage<PlayerReadDto>>(content);

        if (response.IsSuccessStatusCode && dto != null)
        {
            authUser.Uuid = dto.Data!.Uuid;
            authUser.Name = dto.Data.Name;
            authUser.AccessToken = dto.Data!.AccessToken;
            authUser.Has2Fa = false; //dto.Data!.Has2Fa;
            authUser.ExpiredDate = dto.Data!.ExpiredDate;
            authUser.TextureUrl = dto.Data.TextureSkinUrl;

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

        _maxFileCount.OnNext(_progressFilesCount);

        var tasks = files.Select(file =>
            DownloadFileWithRetry(installationDirectory, file, throttler, cancellationToken));

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
            catch(IOException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                if (attempt == 3)
                    throw;
            }
        }
    }

    internal async Task<(Stream Stream, long Bytes)> GetNewLauncher(string guid)
    {
        var url = $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/file/{guid}";

        var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to download file");

        var bytes = response.Content.Headers.ContentLength.GetValueOrDefault(); // Здесь мы получаем размер потока
        var stream = await response.Content.ReadAsStreamAsync(); // Здесь мы получаем поток

        return (stream, bytes);
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

            string localPath = Path.Combine(installationDirectory,
                file.Directory.TrimStart(Path.DirectorySeparatorChar).TrimStart('\\'));
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
            _progressChanged.OnNext(_progress);
            _loadedFilesCount.OnNext(_finishedFilesCount);
            Debug.WriteLine($"{_finishedFilesCount}/{_progressFilesCount}");
        }
        catch (IOException ex) {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            throttler.Release();
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

        var result = JsonConvert.DeserializeObject<ResponseMessage<DiscordRpcReadDto?>>(content);

        return result?.Data;
    }

    public async Task<IVersionFile?> GetActualVersion(OsType osType, Architecture osArch)
    {
        var response = await _httpClient.GetAsync($"/api/v1/launcher").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var result = JsonConvert.DeserializeObject<ResponseMessage<Dictionary<string, LauncherVersion?>?>>(content);

        if (result?.Data is null || result?.Data.Count == 0)
        {
            return null;
        }

        var osName = GetOsName(osType, osArch);

        return result!.Data.FirstOrDefault(c => c.Key == osName).Value;
    }

    private string GetOsName(OsType osType, Architecture osArch)
    {
        StringBuilder versionBuilder = new StringBuilder();

        switch (osType)
        {
            case OsType.Undefined:
                throw new ArgumentOutOfRangeException(nameof(osType), osType, null);
                break;
            case OsType.Linux:
                versionBuilder.Append("linux");
                break;
            case OsType.OsX:
                versionBuilder.Append("osx");
                break;
            case OsType.Windows:
                versionBuilder.Append("win");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(osType), osType, null);
        }

        versionBuilder.Append('-');
        versionBuilder.Append(osArch.ToString().ToLower());

        return versionBuilder.ToString();
    }
}
