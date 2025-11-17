
using Gml.Dto.Files;
using Gml.Dto.Profile;

namespace Gml.Client.Helpers.Files;

public class BaseFileUpdateHandler : IFileUpdateHandler
{
    public virtual async Task<FileValidationResult> ValidateFilesAsync(
        ProfileReadInfoDto profileInfo,
        string rootDirectory)
    {
        var localFiles = new List<ProfileFileReadDto>();
        var profilePath = Path.Combine(rootDirectory, profileInfo.ReleativePath);

        if (!Directory.Exists(profilePath))
        {
            return new FileValidationResult
            {
                FilesToDelete = localFiles,
                FilesToUpdate = profileInfo.Files
            };

        }

        var files = Directory.Exists(profilePath)
            ? Directory.GetFiles(profilePath, "*.*", SearchOption.AllDirectories)
            : [];

        var assetsPath = Path.Combine(rootDirectory, "assets");
        var assetsFiles = Directory.Exists(assetsPath)
            ? Directory.GetFiles(assetsPath, "*.*", SearchOption.AllDirectories)
            : [];

        var runtimePath = Path.Combine(rootDirectory, "runtime");
        var runtimeFiles = Directory.Exists(runtimePath)
            ? Directory.GetFiles(runtimePath, "*.*", SearchOption.AllDirectories)
            : [];

        foreach (var filePath in files.Concat(assetsFiles).Concat(runtimeFiles))
        {
            var relativePath = Path.GetRelativePath(rootDirectory, filePath);
            var fileInfo = new FileInfo(filePath);
            // Используем Path.DirectorySeparatorChar для нормализации пути

            var directory = Path.Combine(
                Path.GetDirectoryName(relativePath)?.Replace('\\', Path.DirectorySeparatorChar)
                    .Replace('/', Path.DirectorySeparatorChar)
                ?? string.Empty,
                fileInfo.Name);

            localFiles.Add(new ProfileFileReadDto
            {
                Name = fileInfo.Name,
                Directory = Path.DirectorySeparatorChar + SystemIoProcedures.NormalizePath(directory),
                Size = fileInfo.Length,
            });
        }

        var result = new FileValidationResult
        {
            FilesToDelete = localFiles,
            FilesToUpdate = profileInfo.Files
        };

        return result;
    }
}
