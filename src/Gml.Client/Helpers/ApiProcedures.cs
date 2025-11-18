using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using DiscordRPC;
using Gml.Client.Interfaces;
using Gml.Client.Models;
using Gml.Domains.Launcher;
using Gml.Dto.Files;
using Gml.Dto.Integration;
using Gml.Dto.Messages;
using Gml.Dto.Mods;
using Gml.Dto.News;
using Gml.Dto.Player;
using Gml.Dto.Profile;
using Gml.Dto.Texture;
using Gml.Dto.User;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Storage;
using GmlCore.Interfaces.User;
using Newtonsoft.Json;

namespace Gml.Client.Helpers;

public class ApiProcedures
{
    private readonly Dictionary<string, List<ProfileFileWatcher>> _fileWatchers = new();
    private readonly HttpClient _httpClient;
    private readonly ISubject<int> _loadedFilesCount = new Subject<int>();
    private readonly ISubject<int> _maxFileCount = new Subject<int>();
    private readonly OsType _osType;

    private readonly ISubject<int> _progressChanged = new Subject<int>();
    private readonly ISubject<long> _downloadedBytesDelta = new Subject<long>();
    private (DiscordRpcClient? Client, DiscordRpcReadDto? ClientInfo)? _discordRpcClient;
    private int _finishedFilesCount;
    private int _progress;

    private int _progressFilesCount;

    public ApiProcedures(HttpClient httpClient, OsType osType)
    {
        _httpClient = httpClient;
        _osType = osType;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"Gml.Launcher-Client-{nameof(GmlClientManager)}/1.0 " +
            $"(OS: {RuntimeInformation.OSDescription.Replace(";", ",")}; " +
            $"OSArchitecture: {RuntimeInformation.OSArchitecture}; " +
            $"ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}; " +
            $"FrameworkDescription: {RuntimeInformation.FrameworkDescription.Replace(";", ",")}; " +
            $".NET: {Environment.Version.ToString(3)};)");
    }

    public IObservable<int> ProgressChanged => _progressChanged;
    public IObservable<int> MaxFileCount => _maxFileCount;
    public IObservable<int> LoadedFilesCount => _loadedFilesCount;
    public IObservable<long> DownloadedBytesDelta => _downloadedBytesDelta;
    internal event EventHandler<string>? FileAdded;

    public async void SaveJsonResponse(string? content, string savePath, string filename = "response")
    {
        try
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            await File.WriteAllTextAsync(Path.Combine(savePath, $"{filename}.json"), content).ConfigureAwait(false);
#if DEBUG
            Debug.WriteLine($"Successfully wrote response to {Path.Combine(savePath, $"{filename}.json")}");
#endif
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Failed to write to {filename}.json: {ex.Message}");
#endif
            SentrySdk.CaptureException(ex);
        }
    }

    [Obsolete("Use method with accessToken")]
    public Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles()
    {
        return GetProfiles(string.Empty);
    }

    public async Task<ResponseMessage<List<ProfileReadDto>>> GetProfiles(string accessToken, string savePath = null)
    {
#if DEBUG
        Debug.WriteLine("Calling GetProfiles()");
#endif
        Debug.Write("Load profiles: ");
        const int maxRetries = 3;

        for (int retryCount = 0; retryCount < maxRetries; retryCount++)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync("/api/v1/profiles").ConfigureAwait(false);

                Debug.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

                if (!response.IsSuccessStatusCode)
                    return new ResponseMessage<List<ProfileReadDto>>();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
                Debug.WriteLine($"Response content: {content}");
#endif
                if (savePath != null)
                {
                    SaveJsonResponse(content, savePath, "profiles");
                }

                return JsonConvert.DeserializeObject<ResponseMessage<List<ProfileReadDto>>>(content)
                       ?? new ResponseMessage<List<ProfileReadDto>>();
            }
            catch (Exception ex) when (retryCount < maxRetries - 1)
            {
#if DEBUG
                Debug.WriteLine($"Exception on attempt {retryCount + 1}: {ex.Message}");
                SentrySdk.CaptureException(ex);
#endif
            }
        }

        throw new Exception("Failed to load profiles after maximum retry attempts.");
    }

    public async Task<ResponseMessage<ProfileReadInfoDto?>?> GetProfileInfo(ProfileCreateInfoDto profileCreateInfoDto, string savePath = null)
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

        if (savePath != null)
        {
            SaveJsonResponse(content, savePath, "profileInfo");
        }

        Debug.Write("Profile loaded");

        var dto = JsonConvert.DeserializeObject<ResponseMessage<ProfileReadInfoDto?>>(content);
#if DEBUG
        Debug.WriteLine(dto != null ? "Profile info loaded successfully." : "Failed to deserialize profile info.");
#endif
        return dto;
    }

    public Task<Process> GetProcess(ProfileReadInfoDto profileDto, string installationDirectory, OsType osType, bool isOffline = false)
    {
#if DEBUG
        Debug.WriteLine("Calling GetProcess()");
#endif
        var process = GetStartProcess(profileDto, installationDirectory, osType, isOffline);
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

    private Process GetStartProcess(ProfileReadInfoDto profileDto, string installationDirectory, OsType osType, bool isOffline = false)
    {
#if DEBUG
        Debug.WriteLine("Calling GetStartProcess()");
#endif
        // var profilePath = installationDirectory + @"\clients\" + profileDto.ProfileName;
        var profilePath = Path.Combine(installationDirectory, profileDto.ReleativePath);

        Dictionary<string, string> parameters;

        if (!isOffline)
        {
            parameters = new Dictionary<string, string>
            {
                { "{localPath}", installationDirectory },
                { "{authEndpoint}", $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/integrations/authlib/minecraft" }
            };
        }
        else
        {
            parameters = new Dictionary<string, string>
            {
                { " -javaagent:{localPath}/clients/1710/libraries/custom/authlib-injector-1.2.5-alpha-1.jar={authEndpoint}", string.Empty },
                { "{localPath}", installationDirectory },
            };
        }

        foreach (var parameter in parameters)
            profileDto.Arguments = profileDto.Arguments.Replace(parameter.Key, parameter.Value, StringComparison.CurrentCultureIgnoreCase);

        var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = SystemIoProcedures.NormalizePath(profileDto.JavaPath).Replace("{localPath}", installationDirectory),
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
            case OsType.OsX:
                var chmodStartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x '{startInfoFileName}\"'"
                };
                Process.Start(chmodStartInfo);
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

    public async Task<(ILauncherUser User, string Message, IEnumerable<string> Details)> Auth(string accessToken)
    {
#if DEBUG
        Debug.WriteLine("Calling Auth(string accessToken)");
#endif
        var model = JsonConvert.SerializeObject(new BaseUserPassword
        {
            AccessToken = accessToken
        });

        var authUser = new AuthLauncherUser();

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

        if(dto is not null && dto.Message.Contains("2FA"))
        {
            authUser.Has2Fa = true;
        }

#if DEBUG
        Debug.WriteLine("Authentication (token) failed.");
#endif
        return (authUser, dto?.Message ?? string.Empty, dto?.Errors ?? []);
    }

    public async Task<(ILauncherUser User, string Message, IEnumerable<string> Details)> AuthWith2Fa(string login, string password,
        string hwid, string twoFactorCode)
    {
#if DEBUG
        Debug.WriteLine($"Sending 2FA verification request for user: {login} with code: {twoFactorCode}");
#endif
        var model = JsonConvert.SerializeObject(new BaseUserPassword
        {
            Login = login,
            Password = password,
            TwoFactorCode = twoFactorCode,
            AccessToken = string.Empty
        });

        var authUser = new AuthLauncherUser
        {
            Name = login
        };

        var data = new StringContent(model, Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Add("X-HWID", hwid);
        var response = await _httpClient.PostAsync("/api/v1/integrations/auth/signin", data).ConfigureAwait(false);
        _httpClient.DefaultRequestHeaders.Remove("X-HWID");
        authUser.IsAuth = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine($"2FA Auth Response: {content}");
#endif

        var dto = JsonConvert.DeserializeObject<ResponseMessage<PlayerReadDto>>(content);

        if (response.IsSuccessStatusCode && dto?.Data != null)
        {
            authUser.Uuid = dto.Data.Uuid;
            authUser.Name = dto.Data.Name;
            authUser.AccessToken = dto.Data.AccessToken;
            authUser.Has2Fa = false;
            authUser.ExpiredDate = dto.Data.ExpiredDate;
            authUser.TextureUrl = dto.Data.TextureSkinUrl;

            return (authUser, string.Empty, []);
        }

        if(dto is not null && dto.Message.Contains("2FA"))
        {
            authUser.Has2Fa = true;
        }

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
        //Debug.WriteLine("All files downloaded.");
#endif
    }

    private async Task DownloadFileWithRetry(string installationDirectory, ProfileFileReadDto file,
        SemaphoreSlim throttler, CancellationToken cancellationToken = default)
    {
        // Try to download file up to 5 times
        for (var attempt = 1; attempt <= 5; attempt++)
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
            var localPath = Path.Combine(installationDirectory,
                SystemIoProcedures.NormalizePath(file.Directory));
            await EnsureDirectoryExists(localPath);

            if (IsOptionalMod(localPath))
            {
                localPath = ToggleOptionalMod(localPath);
            }

            var url = $"{_httpClient.BaseAddress.AbsoluteUri}api/v1/file/{file.Hash}";

            await using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
            {
                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    using var stream = await response.Content.ReadAsStreamAsync();
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, read, cancellationToken);
                        _downloadedBytesDelta.OnNext(read);
                    }
                }

                #if DEBUG
                if (fs.Length != file.Size)
                {
                    SentrySdk.CaptureException(new Exception($"Файл не был полностью загружен: {file.Directory}"));
                }
                #endif
            }

            _finishedFilesCount++;
            _progress = Convert.ToInt16(_finishedFilesCount * 100 / _progressFilesCount);
            _progressChanged.OnNext(_progress);
            _loadedFilesCount.OnNext(_finishedFilesCount);
#if DEBUG
            //Debug.WriteLine($"{_finishedFilesCount}/{_progressFilesCount} files downloaded [{file.Directory}].");
#endif
        }
        catch (IOException ex)
        {
#if DEBUG
            Debug.WriteLine($"IOException during download {file.Directory}: {ex.Message}");
#endif
            throw;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Exception during download {file.Directory}: {ex.Message}");
#endif
            Console.WriteLine(ex);
        }
        finally
        {
            throttler.Release();
        }
    }

    public string ToggleOptionalMod(string localPath)
    {
        if (IsOptionalMod(localPath))
        {
            return $"{localPath}.disabled";
        }

        return localPath.Replace(".disabled", string.Empty);
    }

    public static bool IsOptionalMod(string localPath)
    {
        return localPath.Contains("-optional-mod.jar");
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
            result.Data.FullSkinUrl = result.Data.ExternalTextureSkinUrl ??
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

    public async Task<ResponseMessage<List<ModsDetailsInfoDto>>> GetOptionalModsInfo(string accessToken)
    {
#if DEBUG
        Debug.WriteLine("Calling GetOptionalMods()");
#endif
        Debug.Write("Load profiles: ");
        if (_httpClient.DefaultRequestHeaders.TryGetValues("Authorization", out _))
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.GetAsync("/api/v1/mods/details").ConfigureAwait(false);

        Debug.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return new ResponseMessage<List<ModsDetailsInfoDto>>();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine(response.IsSuccessStatusCode
            ? $"Mods loaded successfully: {content}"
            : "Failed to load profiles.");
#endif
        return JsonConvert.DeserializeObject<ResponseMessage<List<ModsDetailsInfoDto>>>(content)
               ?? new ResponseMessage<List<ModsDetailsInfoDto>>();
    }

    public async Task<ResponseMessage<List<ModReadDto>>> GetOptionalMods(string profileName, string accessToken)
    {
#if DEBUG
        Debug.WriteLine("Calling GetOptionalMods()");
#endif
        Debug.Write("Load profiles: ");
        if (_httpClient.DefaultRequestHeaders.TryGetValues("Authorization", out _))
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await _httpClient.GetAsync($"/api/v1/profiles/{profileName}/mods/optionals")
            .ConfigureAwait(false);

        Debug.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return new ResponseMessage<List<ModReadDto>>();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine(response.IsSuccessStatusCode
            ? $"Mods loaded successfully: {content}"
            : "Failed to load profiles.");
#endif
        return JsonConvert.DeserializeObject<ResponseMessage<List<ModReadDto>>>(content)
               ?? new ResponseMessage<List<ModReadDto>>();
    }

    public string ToggleOptionalMod(string localPath, bool isEnabled)
    {
        return isEnabled ? localPath.Replace(".disabled", string.Empty) : $"{localPath}.disabled";
    }

    public async Task<ResponseMessage<List<NewsReadDto>>> GetNews()
    {
#if DEBUG
        Debug.WriteLine("Calling GetNews()");
#endif

        var response = await _httpClient.GetAsync($"/api/v1/integrations/news/list")
            .ConfigureAwait(false);

        Debug.WriteLine(response.IsSuccessStatusCode ? "Success load" : "Failed load");

        if (!response.IsSuccessStatusCode)
            return new ResponseMessage<List<NewsReadDto>>();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine(response.IsSuccessStatusCode
            ? $"Mods loaded successfully: {content}"
            : "Failed to load profiles.");
#endif
        return JsonConvert.DeserializeObject<ResponseMessage<List<NewsReadDto>>>(content)
               ?? new ResponseMessage<List<NewsReadDto>>();
    }

    public static async Task<bool> CheckBackend(string hostUrl)
    {
#if DEBUG
        Debug.WriteLine("Calling CheckBackend()");
#endif
        using var client = new HttpClient();
        var response = await client.GetAsync($"{hostUrl}").ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            response = await client.GetAsync($"{hostUrl}/health").ConfigureAwait(false);
        }

        Debug.WriteLine(response.IsSuccessStatusCode ? "Success check" : "Failed check");

#if DEBUG
        Debug.WriteLine(response.IsSuccessStatusCode
            ? $"Backend pinged successfully {response.StatusCode}"
            : "Failed ping");
#endif
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> ReadJsonResponse(string directory, string fileName = "response")
    {
        string path = Path.Combine(directory, $"{fileName}.json");
        if (!File.Exists(path))
        {
#if DEBUG
            Debug.WriteLine($"{fileName}.json file not found");
#endif
            return null;
        }
        string content = await File.ReadAllTextAsync(path).ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine($"Read content from {fileName}.json: {content}");
#endif
        return content;
    }
}
