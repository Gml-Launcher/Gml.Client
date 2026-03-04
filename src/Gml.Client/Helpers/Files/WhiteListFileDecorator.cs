
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Gml.Dto.Files;
using Gml.Dto.Profile;

namespace Gml.Client.Helpers.Files;

public class WhiteListFileDecorator : IFileUpdateHandler
{
    private readonly IFileUpdateHandler _handler;

    public WhiteListFileDecorator(IFileUpdateHandler handler)
    {
        _handler = handler;
    }

    public async Task<FileValidationResult> ValidateFilesAsync(ProfileReadInfoDto profileInfo, string rootDirectory)
    {
        var result = await _handler.ValidateFilesAsync(profileInfo, rootDirectory);

        result.FilesToDelete = result.FilesToDelete.Where(file => !ExistsInWhiteList(profileInfo, file));
        result.FilesToUpdate = result.FilesToUpdate.Where(file => ShouldKeepForUpdate(profileInfo, file, rootDirectory));

        return result;
    }

    private bool FileExists(ProfileFileReadDto file, string rootDirectory)
    {
        var fullName = Path.Combine(rootDirectory, SystemIoProcedures.NormalizePath(file.Directory));
        // Для опциональных модов файл может быть сохранен с суффиксом ".disabled"
        if (File.Exists(fullName)) return true;
        var disabledPath = fullName + ".disabled";
        return File.Exists(disabledPath);
    }

    private static bool ExistsInWhiteList(ProfileReadInfoDto profileInfo, ProfileFileReadDto file)
    {
        return profileInfo.WhiteListFiles.Any(w =>
            SystemIoProcedures.NormalizePath(w.Directory).Equals(
                SystemIoProcedures.NormalizePath(file.Directory),
                StringComparison.OrdinalIgnoreCase));
    }

    private bool ShouldKeepForUpdate(ProfileReadInfoDto profileInfo, ProfileFileReadDto file, string rootDirectory)
    {
        var inWhiteList = ExistsInWhiteList(profileInfo, file);
        var fullPath = Path.Combine(rootDirectory, SystemIoProcedures.NormalizePath(file.Directory));

        var exists = File.Exists(fullPath) || File.Exists(fullPath + ".disabled");
        if (!exists) return true;

        if (!inWhiteList) return true;

        if (string.IsNullOrWhiteSpace(file.Hash)) return false;

        var actualLocalPath = File.Exists(fullPath) ? fullPath : fullPath + ".disabled";

        try
        {
            using var algorithm = SHA1.Create();
            var localHash = SystemHelper.CalculateFileHash(actualLocalPath, algorithm);
            return !string.Equals(localHash, file.Hash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true;
        }
    }
}
