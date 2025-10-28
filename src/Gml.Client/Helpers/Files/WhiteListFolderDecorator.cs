
using Gml.Dto.Files;
using Gml.Dto.Profile;

namespace Gml.Client.Helpers.Files;

public class WhiteListFolderDecorator : IFileUpdateHandler
{
    private readonly IFileUpdateHandler _handler;

    public WhiteListFolderDecorator(IFileUpdateHandler handler)
    {
        _handler = handler;
    }

    public async Task<FileValidationResult> ValidateFilesAsync(ProfileReadInfoDto profileInfo, string rootDirectory)
    {
        var result = await _handler.ValidateFilesAsync(profileInfo, rootDirectory);

        result.FilesToDelete = result.FilesToDelete.Where(file =>
            !profileInfo.WhiteListFolders.Any(folder =>
                SystemIoProcedures.NormalizePath(file.Directory).StartsWith(
                    SystemIoProcedures.NormalizePath(Path.Combine("clients", profileInfo.ProfileName, folder.Path)),
                    StringComparison.OrdinalIgnoreCase)));

        result.FilesToUpdate = result.FilesToUpdate.Where(file =>
        {
            var fullPath = Path.Combine(rootDirectory, SystemIoProcedures.NormalizePath(file.Directory));

            return FileNotExists(fullPath) || NotExistsInWhiteList(profileInfo, file);
        });

        return result;
    }

    private static bool NotExistsInWhiteList(ProfileReadInfoDto profileInfo, ProfileFileReadDto file)
    {
        return !profileInfo.WhiteListFolders.Any(folder =>
            SystemIoProcedures.NormalizePath(file.Directory).StartsWith(
                SystemIoProcedures.NormalizePath(Path.Combine("clients", profileInfo.ProfileName, folder.Path)),
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool FileNotExists(string fullPath)
    {
        return !File.Exists(fullPath);
    }
}
