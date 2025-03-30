using Gml.Web.Api.Dto.Files;
using Gml.Web.Api.Dto.Profile;

namespace Gml.Client.Helpers.Files;

public class BaseFileUpdateHandler : IFileUpdateHandler
{
    public virtual async Task<FileValidationResult> ValidateFilesAsync(
        ProfileReadInfoDto profileInfo,
        string rootDirectory)
    {
        var localFiles = new List<ProfileFileReadDto>();
        var profilePath = Path.Combine(rootDirectory, "clients", profileInfo.ProfileName);

        if (!Directory.Exists(profilePath))
        {
            return new FileValidationResult
            {
                FilesToDelete = localFiles,
                FilesToUpdate = profileInfo.Files
            };

        }

        var files = Directory.GetFiles(profilePath, "*.*", SearchOption.AllDirectories);
        var assetsFiles = Directory.GetFiles(Path.Combine(rootDirectory, "assets"), "*.*", SearchOption.AllDirectories);
        var runtimeFiles =
            Directory.GetFiles(Path.Combine(rootDirectory, "runtime"), "*.*", SearchOption.AllDirectories);

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
