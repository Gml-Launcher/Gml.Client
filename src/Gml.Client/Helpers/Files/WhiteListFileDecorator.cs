using Gml.Web.Api.Dto.Profile;

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

        result.FilesToDelete = result.FilesToDelete.Where(file =>
            !profileInfo.WhiteListFiles.Any(w =>
                NormalizePath(w.Directory).TrimStart('\\').Equals(
                    NormalizePath(file.Directory).TrimStart('\\'),
                    StringComparison.OrdinalIgnoreCase)));

        result.FilesToUpdate = result.FilesToUpdate.Where(file =>
            !profileInfo.WhiteListFiles.Any(w =>
                NormalizePath(w.Directory).TrimStart('\\').Equals(
                    NormalizePath(file.Directory).TrimStart('\\'),
                    StringComparison.OrdinalIgnoreCase)));

        return result;
    }

    private string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }
}
