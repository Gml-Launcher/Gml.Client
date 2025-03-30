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
                SystemIoProcedures.NormalizePath(w.Directory).Equals(
                    SystemIoProcedures.NormalizePath(file.Directory),
                    StringComparison.OrdinalIgnoreCase)));

        result.FilesToUpdate = result.FilesToUpdate.Where(file =>
            !profileInfo.WhiteListFiles.Any(w =>
                SystemIoProcedures.NormalizePath(w.Directory).Equals(
                    SystemIoProcedures.NormalizePath(file.Directory),
                    StringComparison.OrdinalIgnoreCase)));

        return result;
    }
}
