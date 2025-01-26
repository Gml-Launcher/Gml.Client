using System.Collections.Concurrent;
using System.Security.Cryptography;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Profile;

namespace Gml.Client.Helpers;

public class SystemIoProcedures
{
    private const long _oneHundredMB = 100 * 1024 * 1024;
    private readonly string _installationDirectory;
    private readonly OsType _osType;

    public SystemIoProcedures(string installationDirectory, OsType osType)
    {
        _installationDirectory = installationDirectory;
        _osType = osType;
    }

    public List<ProfileFileReadDto> FindErroneousFiles(
        ProfileReadInfoDto profileInfo,
        string installationDirectory)
    {
        // Кэширование списков файлов и белого списка
        var files = profileInfo.Files.ToList();
        var whiteListFiles = profileInfo.WhiteListFiles.ToHashSet();
        var errorFiles = new ConcurrentBag<ProfileFileReadDto>();

        Parallel.ForEach(files, downloadingFile =>
        {
            if (_osType == OsType.Windows)
                downloadingFile.Directory = downloadingFile.Directory.Replace('/', Path.DirectorySeparatorChar)
                    .TrimStart(Path.DirectorySeparatorChar);

            var localPath = Path.Combine(installationDirectory,
                downloadingFile.Directory.TrimStart('\\').TrimStart(Path.DirectorySeparatorChar));

            if (FileExists(localPath))
            {
                if (new FileInfo(localPath).Length >= _oneHundredMB) return;

                var hashIsCorrect = SystemHelper.CalculateFileHash(localPath, new SHA256Managed()) ==
                                    downloadingFile.Hash;
                if (hashIsCorrect) return;

                if (!whiteListFiles.Any(c => c.Directory.Contains(downloadingFile.Directory)))
                    errorFiles.Add(downloadingFile);
            }
            else
            {
                errorFiles.Add(downloadingFile);
            }
        });

        return errorFiles.OrderBy(c => c.Size).ToList();
    }

    private static bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public Task RemoveFiles(ProfileReadInfoDto profileInfo)
    {
        try
        {
            List<string> allowedPaths =
            [
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName, "saves")),
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName, "logs")),
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName,
                    "crash-reports"))
            ];

            allowedPaths.AddRange(profileInfo.WhiteListFolders.Select(path =>
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName,
                    Path.Combine(path.Path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)))))
            );

            var profilePath =
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName));

            var directoryInfo = new DirectoryInfo(profilePath);

            if (!directoryInfo.Exists) directoryInfo.Create();

            var localFiles = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

            var hashSet = profileInfo.Files
                .Select(f => Path.GetFullPath(GetRealFilePath(_installationDirectory, f)))
                .Concat(profileInfo.WhiteListFiles
                    .Select(wf => Path.GetFullPath(GetRealFilePath(_installationDirectory, wf))))
                .ToHashSet();

            bool IsNeedRemove(FileInfo fileInfo)
            {
                if (fileInfo.Length == 0)
                {
                    return true;
                }

                if (!hashSet.Contains(fileInfo.FullName) && !allowedPaths.Any(path => fileInfo.FullName.StartsWith(path)))
                {
                    return CompareHashOptionalMods(profileInfo, fileInfo);
                }

                return CompareHashOptionalMods(profileInfo, fileInfo);
            }

            foreach (var file in localFiles.Where(IsNeedRemove))
                try
                {
                    file.Delete();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        return Task.CompletedTask;
    }

    private static bool CompareHashOptionalMods(ProfileReadInfoDto profileInfo, FileInfo fileInfo)
    {
        if (!ApiProcedures.IsOptionalMod(fileInfo.FullName))
        {
            return profileInfo.Files.Any(c => c.Name == fileInfo.Name) == false;
        }

        using var hash = new SHA256Managed();
        var fileHash = SystemHelper.CalculateFileHash(fileInfo.FullName, hash);

        return profileInfo.Files.All(c => c.Hash != fileHash);
    }

    private string GetRealFilePath(string installationDirectory, ProfileFileReadDto file)
    {
        if (_osType == OsType.Windows)
            file.Directory = file.Directory.Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

        return Path.Combine(installationDirectory,
            file.Directory.TrimStart('\\').TrimStart(Path.DirectorySeparatorChar));
    }
}
