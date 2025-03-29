using System.Collections.Concurrent;
using System.Security.Cryptography;
using Gml.Client.Helpers.Files;
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

                var hashIsCorrect = SystemHelper.CalculateFileHash(localPath, SHA1.Create()) ==
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

    public Task RemoveFiles(string rootDirectory, IEnumerable<ProfileFileReadDto> files)
    {
        foreach (var file in files)
        {
            try
            {
                File.Delete(Path.Combine(rootDirectory, file.Directory.TrimStart('\\')));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return Task.CompletedTask;
    }

    [Obsolete(
        "Данный метод не рекомендуется к использованию. Используйте ValidateFilesAsync, который  вернет файлы для обновления и удаления")]

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
                // Not remove allowed paths
                if (allowedPaths.Any(path => fileInfo.FullName.StartsWith(path)))
                {
                    return false;
                }

                // Remove empty files
                if (fileInfo.Length == 0)
                {
                    return true;
                }

                var filesByName = profileInfo.Files
                    .Where(c => c.Name == fileInfo.Name)
                    .ToArray();

                if (profileInfo.WhiteListFiles.Any(c => fileInfo.FullName.EndsWith(c.Directory)))
                {
                    return false;
                }

                if (filesByName.Any(c => c.Size != fileInfo.Length))
                {
                    return true;
                }

                return filesByName.Any(c => HasFileByHash(c, fileInfo)) == false;

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

    private static bool HasFileByHash(ProfileFileReadDto profileFile, FileInfo fileInfo)
    {
        using var hash = SHA1.Create();

        return profileFile.Hash == SystemHelper.CalculateFileHash(fileInfo.FullName, hash);;
    }

    private string GetRealFilePath(string installationDirectory, ProfileFileReadDto file)
    {
        if (_osType == OsType.Windows)
            file.Directory = file.Directory.Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

        return Path.Combine(installationDirectory,
            file.Directory.TrimStart('\\').TrimStart(Path.DirectorySeparatorChar));
    }

    public async Task<(ProfileFileReadDto[] ToUpdate, ProfileFileReadDto[] ToDelete)> ValidateFilesAsync(
        ProfileReadInfoDto profileInfo,
        string rootDirectory)
    {
        IFileUpdateHandler handler = new BaseFileUpdateHandler();
        handler = new HashValidationDecorator(handler);
        handler = new WhiteListFileDecorator(handler);
        handler = new WhiteListFolderDecorator(handler);

        var result = await handler.ValidateFilesAsync(profileInfo, rootDirectory);

        return (result.FilesToUpdate.ToArray(), result.FilesToDelete.ToArray());
    }
}
