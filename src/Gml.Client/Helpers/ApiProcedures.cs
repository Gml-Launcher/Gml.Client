using System.Diagnostics;
using System.Net;
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
using GmlCore.Interfaces.User;
using Newtonsoft.Json;
using IUser = Gml.Client.Models.IUser;

namespace Gml.Client.Helpers;

public class ApiProcedures
{
    private readonly Dictionary<string, List<ProfileFileWatcher>> _fileWatchers = new();
    private readonly HttpClient _httpClient;
    private readonly ISubject<int> _loadedFilesCount = new Subject<int>();
    private readonly ISubject<int> _maxFileCount = new Subject<int>();
    private readonly OsType _osType;

    private readonly ISubject<int> _progressChanged = new Subject<int>();
    private (DiscordRpcClient? Client, DiscordRpcReadDto? ClientInfo)? _discordRpcClient;
    private int _finishedFilesCount;
    private int _progress;

    private int _progressFilesCount;

    public ApiProcedures(HttpClient httpClient, OsType osType)
    {
        _httpClient = httpClient;
        _osType = osType;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"Gml.Launcher-Client-{nameof(GmlClientManager)}/1.0 (OS: {Environment.OSVersion};)");
    }

    public IObservable<int> ProgressChanged => _progressChanged;
    public IObservable<int> MaxFileCount => _maxFileCount;
    public IObservable<int> LoadedFilesCount => _loadedFilesCount;
    internal event EventHandler<string>? FileAdded;

    public async Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles()
    {
#if DEBUG
        Debug.WriteLine("Calling GetProfiles()");
#endif
        Debug.Write("Load profiles: ");
        var response = await _httpClient.GetAsync("/api/v1/profiles").ConfigureAwait(false);

        Debug.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return new ResponseMessage<List<ProfileReadDto>>();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine(response.IsSuccessStatusCode
            ? $"Profiles loaded successfully: {content}"
            : "Failed to load profiles.");
#endif
        return JsonConvert.DeserializeObject<ResponseMessage<List<ProfileReadDto>>>(content)
               ?? new ResponseMessage<List<ProfileReadDto>>();
    }

    public async Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto)
    {
#if DEBUG
        Debug.WriteLine("Calling GetProfileInfo()");
#endif
        Debug.Write("Get profile info");
        var model = JsonConvert.SerializeObject(profileCreateInfoDto);

        var data = new StringContent(model, Encoding.UTF8, "application/json");

        var clientMessage = new HttpRequestMessage
        {
            Content = data,
            RequestUri = new Uri(string.Concat(_httpClient.BaseAddress, "api/v1/profiles/info")),
            Method = HttpMethod.Post
        };

        clientMessage.Headers.Add("Authorization", profileCreateInfoDto.UserAccessToken);

        var response = await _httpClient.SendAsync(clientMessage).ConfigureAwait(false);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
#if DEBUG
            Debug.WriteLine("Unauthorized access when fetching profile info.");
#endif
            throw new UnauthorizedAccessException();
        }

        if (!response.IsSuccessStatusCode)
        {
#if DEBUG
            Debug.WriteLine("Failed to fetch profile info.");
#endif
            return null;
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        Debug.Write("Profile loaded");

        var dto = JsonConvert.DeserializeObject<ResponseMessage<ProfileReadInfoDto?>>(content);
#if DEBUG
        Debug.WriteLine(dto != null ? "Profile info loaded successfully." : "Failed to deserialize profile info.");
#endif
        return dto;
    }

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, string installationDirectory, OsType osType)
    {
#if DEBUG
        Debug.WriteLine("Calling GetProcess()");
#endif
        var process = GetStartProcess(profileDto, installationDirectory, osType);
#if DEBUG
        Debug.WriteLine("Process created successfully.");
#endif
        return Task.FromResult(process);
    }

    private List<ProfileFileReadDto> GetAllowFiles(ProfileReadInfoDto profileDto)
    {
#if DEBUG
        Debug.WriteLine("Calling GetAllowFiles()");
#endif
        return profileDto.Files.Where(c => c.Directory.Contains(@"\mods\") || c.Directory.Contains("/mods/")).ToList();
    }

    private Process GetStartProcess(ProfileReadInfoDto profileDto, string installationDirectory, OsType osType)
    {
#if DEBUG
        Debug.WriteLine("Calling GetStartProcess()");
#endif
        // var profilePath = installationDirectory + @"\clients\" + profileDto.ProfileName;
        var profilePath = Path.Combine(installationDirectory, "clients", profileDto.ProfileName);

        var parameters = new Dictionary<string, string>
        {
            { "{localPath}", installationDirectory },
            { "{authEndpoint}", $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/authlib/minecraft" }
        };

        foreach (var parameter in parameters)
            profileDto.Arguments = profileDto.Arguments.Replace(parameter.Key, parameter.Value);

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
#if DEBUG
            Debug.WriteLine($"Java file not found: {process.StartInfo.FileName}");
#endif
            throw new FileNotFoundException("Java file not found", process.StartInfo.FileName);
        }

        ChangeProcessRules(process.StartInfo.FileName, osType);
        InitializeFileWatchers(process, profileDto, GetAllowFiles(profileDto), profilePath, installationDirectory);
#if DEBUG
        Debug.WriteLine("Process start configuration initialized successfully.");
#endif
        return process;
    }

    private void ChangeProcessRules(string startInfoFileName, OsType profileDtoOsType)
    {
#if DEBUG
        Debug.WriteLine("Calling ChangeProcessRules()");
#endif
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
#if DEBUG
        Debug.WriteLine("Calling InitializeFileWatchers()");
#endif
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
                modsWatcher
                // assetsWatcher
            ];
        }
    }

    public static async Task<string> GetSentryLink(string hostUrl)
    {
#if DEBUG
        Debug.WriteLine("Calling GetSentryLink()");
#endif
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
#if DEBUG
                Debug.WriteLine("Sentry link retrieved successfully.");
#endif
                return url?.Data?.Url ?? string.Empty;
            }
        }
        catch (Exception exception)
        {
#if DEBUG
            Debug.WriteLine($"Error while getting Sentry link: {exception.Message}");
#endif
            Console.WriteLine(exception);
        }

        return string.Empty;
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string login, string password,
        string hwid)
    {
#if DEBUG
        Debug.WriteLine("Calling Auth(string login, string password, string hwid)");
#endif
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
            authUser.Has2Fa = false;
            authUser.ExpiredDate = dto.Data!.ExpiredDate;
            authUser.TextureUrl =
                $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/texture/skins/{dto.Data.TextureSkinGuid}";

#if DEBUG
            Debug.WriteLine("Authentication success.");
#endif
            return (authUser, string.Empty, Enumerable.Empty<string>());
        }

#if DEBUG
        Debug.WriteLine("Authentication failed.");
#endif
        return (authUser, dto?.Message ?? string.Empty, dto?.Errors ?? []);
    }

    public async Task<(IUser User, string Message, IEnumerable<string> Details)> Auth(string accessToken)
    {
#if DEBUG
        Debug.WriteLine("Calling Auth(string accessToken)");
#endif
        var model = JsonConvert.SerializeObject(new BaseUserPassword
        {
            AccessToken = accessToken
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

#if DEBUG
            Debug.WriteLine("Authentication (token) success.");
#endif
            return (authUser, string.Empty, Enumerable.Empty<string>());
        }

#if DEBUG
        Debug.WriteLine("Authentication (token) failed.");
#endif
        return (authUser, dto?.Message ?? string.Empty, dto?.Errors ?? []);
    }

    public async Task DownloadFiles(string installationDirectory, ProfileFileReadDto[] files, int loadFilesPartCount,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        Debug.WriteLine("Calling DownloadFiles()");
#endif
        _progress = 0;
        _finishedFilesCount = 0;
        _progressFilesCount = files.Length;

        var throttler = new SemaphoreSlim(loadFilesPartCount);

        _maxFileCount.OnNext(_progressFilesCount);

        var tasks = files.Select(file =>
            DownloadFileWithRetry(installationDirectory, file, throttler, cancellationToken));

        // Исполнение всех задач.
        await Task.WhenAll(tasks);
#if DEBUG
        Debug.WriteLine("All files downloaded.");
#endif
    }

    private async Task DownloadFileWithRetry(string installationDirectory, ProfileFileReadDto file,
        SemaphoreSlim throttler, CancellationToken cancellationToken = default)
    {
        // Try to download file up to 3 times
        for (var attempt = 1; attempt <= 3; attempt++)
            try
            {
                await DownloadFile(installationDirectory, file, throttler, cancellationToken);
                return;
            }
            catch (IOException ex)
            {
#if DEBUG
                Debug.WriteLine($"IOException on attempt {attempt}: {ex.Message}");
#endif
                throw;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Exception on attempt {attempt}: {ex.Message}");
#endif
                if (attempt == 3)
                    throw;
            }
    }

    internal async Task<(Stream Stream, long Bytes)> GetNewLauncher(string guid)
    {
#if DEBUG
        Debug.WriteLine($"Calling GetNewLauncher for guid: {guid}");
#endif
        var url = $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/file/{guid}";

        var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
#if DEBUG
            Debug.WriteLine("Failed to download launcher file");
#endif
            throw new Exception("Failed to download file");
        }

        var bytes = response.Content.Headers.ContentLength.GetValueOrDefault(); // Здесь мы получаем размер потока
        var stream = await response.Content.ReadAsStreamAsync(); // Здесь мы получаем поток

#if DEBUG
        Debug.WriteLine("Launcher file downloaded successfully");
#endif

        return (stream, bytes);
    }

    private async Task DownloadFile(string installationDirectory, ProfileFileReadDto file, SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        await throttler.WaitAsync(cancellationToken);

        try
        {
            if (_osType == OsType.Windows)
                file.Directory = file.Directory.Replace('/', Path.DirectorySeparatorChar)
                    .TrimStart(Path.DirectorySeparatorChar);

            var localPath = Path.Combine(installationDirectory,
                file.Directory.TrimStart(Path.DirectorySeparatorChar).TrimStart('\\'));
            await EnsureDirectoryExists(localPath);

            var url = $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/file/{file.Hash}";

            await using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                await using (var stream = await _httpClient.GetStreamAsync(url))
                {
                    await stream.CopyToAsync(fs, cancellationToken);
                }
            }

            _finishedFilesCount++;
            _progress = Convert.ToInt16(_finishedFilesCount * 100 / _progressFilesCount);
            _progressChanged.OnNext(_progress);
            _loadedFilesCount.OnNext(_finishedFilesCount);
#if DEBUG
            Debug.WriteLine($"{_finishedFilesCount}/{_progressFilesCount} files downloaded.");
#endif
        }
        catch (IOException ex)
        {
#if DEBUG
            Debug.WriteLine($"IOException during download: {ex.Message}");
#endif
            throw;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Exception during download: {ex.Message}");
#endif
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
        if (!directoryInfo.Exists) directoryInfo.Create();

        return Task.CompletedTask;
    }

    public Task LoadDiscordRpc()
    {
#if DEBUG
        Debug.WriteLine("Calling LoadDiscordRpc");
#endif
        return GetDiscordRpcClient();
    }

    public async Task UpdateDiscordRpcState(string state)
    {
#if DEBUG
        Debug.WriteLine($"Updating Discord RPC State to: {state}");
#endif
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
#if DEBUG
            Debug.WriteLine("Discord RPC State updated successfully.");
#endif
        }
    }

    private async Task<(DiscordRpcClient? Client, DiscordRpcReadDto? ClientInfo)> GetDiscordRpcClient()
    {
#if DEBUG
        Debug.WriteLine("Fetching Discord RPC Client");
#endif
        if (_discordRpcClient?.Client is null)
        {
            var discordClient = await GetDiscordClient();

            if (discordClient is null || string.IsNullOrEmpty(discordClient.ClientId))
            {
#if DEBUG
                Debug.WriteLine("Failed to retrieve Discord client info or ClientId is empty.");
#endif
                return (null, null);
            }

            var client = new DiscordRpcClient(discordClient.ClientId);
            client.Initialize();

#if DEBUG
            Debug.WriteLine($"DiscordRPC is initialized: {client.IsInitialized}");
#endif
            return (client, discordClient);
        }

        return (null, null);
    }

    public async Task<DiscordRpcReadDto?> GetDiscordClient()
    {
#if DEBUG
        Debug.WriteLine("Calling GetDiscordClient");
#endif
        var response = await _httpClient.GetAsync("/api/v1/integrations/discord").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
#if DEBUG
            Debug.WriteLine("Failed to retrieve Discord client");
#endif
            return null;
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var result = JsonConvert.DeserializeObject<ResponseMessage<DiscordRpcReadDto?>>(content);

#if DEBUG
        Debug.WriteLine("Discord client data retrieved successfully.");
#endif
        return result?.Data;
    }

    public async Task<IPlayerTexture?> GetTexturesByName(string userName)
    {
#if DEBUG
        Debug.WriteLine($"Calling GetTexturesByName for user: {userName}");
#endif
        var response = await _httpClient.GetAsync($"/api/v1/users/info/{userName}").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
#if DEBUG
            Debug.WriteLine($"Failed to get textures for user: {userName}");
#endif
            return null;
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var result = JsonConvert.DeserializeObject<ResponseMessage<PlayerTextureDto?>>(content);

        if (result?.Data is not null)
            result.Data.FullSkinUrl =
                $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/texture/skins/{result.Data.TextureSkinGuid}";

#if DEBUG
        Debug.WriteLine($"Textures for user {userName} retrieved successfully.");
#endif
        return result?.Data;
    }

    public async Task<IVersionFile?> GetActualVersion(OsType osType, Architecture osArch)
    {
#if DEBUG
        Debug.WriteLine("Calling GetActualVersion");
#endif
        var response = await _httpClient.GetAsync("/api/v1/launcher").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
#if DEBUG
            Debug.WriteLine("Failed to get the actual version");
#endif
            return null;
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var result = JsonConvert.DeserializeObject<ResponseMessage<Dictionary<string, LauncherVersion?>?>>(content);

        if (result?.Data is null || result?.Data.Count == 0)
        {
#if DEBUG
            Debug.WriteLine("No version data available");
#endif
            return null;
        }

        var osName = GetOsName(osType, osArch);

#if DEBUG
        Debug.WriteLine("Actual version retrieved successfully.");
#endif
        return result!.Data.FirstOrDefault(c => c.Key == osName).Value;
    }

    private string GetOsName(OsType osType, Architecture osArch)
    {
#if DEBUG
        Debug.WriteLine($"Fetching OS name for type: {osType} and architecture: {osArch}");
#endif
        var versionBuilder = new StringBuilder();

        switch (osType)
        {
            case OsType.Undefined:
                throw new ArgumentOutOfRangeException(nameof(osType), osType, null);
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

#if DEBUG
        Debug.WriteLine($"OS name determined: {versionBuilder}");
#endif
        return versionBuilder.ToString();
    }
}
