
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
        result.FilesToUpdate = result.FilesToUpdate.Where(file => !FileExists(file, rootDirectory) || !ExistsInWhiteList(profileInfo, file));

        return result;
    }

    private bool FileExists(ProfileFileReadDto file, string rootDirectory)
    {
        var fullName = Path.Combine(rootDirectory, SystemIoProcedures.NormalizePath(file.Directory));

        return File.Exists(fullName);
    }

    private static bool ExistsInWhiteList(ProfileReadInfoDto profileInfo, ProfileFileReadDto file)
    {
        return profileInfo.WhiteListFiles.Any(w =>
            SystemIoProcedures.NormalizePath(w.Directory).Equals(
                SystemIoProcedures.NormalizePath(file.Directory),
                StringComparison.OrdinalIgnoreCase));
    }
}
