using System.Collections.Concurrent;
using System.Security.Cryptography;
using Gml.Web.Api.Domains.System;
using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Profile;

namespace Gml.Client.Helpers;

public class SystemIoProcedures
{
    private readonly string _installationDirectory;
    private readonly OsType _osType;
    private const long _oneHundredMB = 100 * 1024 * 1024;

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
            {
                downloadingFile.Directory = downloadingFile.Directory.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            }

            string localPath = Path.Combine(installationDirectory, downloadingFile.Directory.TrimStart('\\').TrimStart(Path.DirectorySeparatorChar));

            if (FileExists(localPath))
            {
                if (new FileInfo(localPath).Length >= _oneHundredMB)
                {
                    return;
                }

                var hashIsCorrect = SystemHelper.CalculateFileHash(localPath, new SHA256Managed()) == downloadingFile.Hash;
                if (hashIsCorrect)
                {
                    return;
                }
            }

            if (!whiteListFiles.Any(c => c.Hash.Equals(downloadingFile.Hash)))
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
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName, "crash-reports"))
            ];

            allowedPaths.AddRange(profileInfo.WhiteListFolders.Select(path =>
                Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName,
                    Path.Combine(path.Path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)))))
                );

            var profilePath = Path.GetFullPath(Path.Combine(_installationDirectory, "clients", profileInfo.ProfileName));

            var directoryInfo = new DirectoryInfo(profilePath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

            var hashSet = profileInfo.Files
                .Select(f => Path.GetFullPath(GetRealFilePath(_installationDirectory, f)))
                .Concat(profileInfo.WhiteListFiles
                    .Select(wf => Path.GetFullPath(GetRealFilePath(_installationDirectory, wf))));

            var exclusionSet = new HashSet<string>(hashSet);

            var filesToRemove = files
                .Where(f => !exclusionSet.Contains(f.FullName) && !allowedPaths.Any(path => f.FullName.StartsWith(path)))
                .ToList();

            foreach (var file in filesToRemove)
            {
                try
                {
                    file.Delete();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        return Task.CompletedTask;
    }

    private string GetRealFilePath(string installationDirectory, ProfileFileReadDto file)
    {
        if (_osType == OsType.Windows)
        {
            file.Directory = file.Directory.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        }

        return Path.Combine(installationDirectory, file.Directory.TrimStart('\\').TrimStart(Path.DirectorySeparatorChar));
    }
}
