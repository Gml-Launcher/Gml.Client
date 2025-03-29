using Gml.Web.Api.Dto.Profile;

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
                NormalizePath(file.Directory.TrimStart('\\')).StartsWith(
                    NormalizePath(Path.Combine("clients", profileInfo.ProfileName, folder.Path.TrimStart('\\'))),
                    StringComparison.OrdinalIgnoreCase)));

        result.FilesToUpdate = result.FilesToUpdate.Where(file =>
            !profileInfo.WhiteListFolders.Any(folder =>
                NormalizePath(file.Directory.TrimStart('\\')).StartsWith(
                    NormalizePath(Path.Combine("clients", profileInfo.ProfileName, folder.Path.TrimStart('\\'))),
                    StringComparison.OrdinalIgnoreCase)));

        return result;
    }

    private string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}
