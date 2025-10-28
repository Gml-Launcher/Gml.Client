using Gml.Dto.Profile;

namespace Gml.Client.Helpers.Files;

public interface IFileUpdateHandler
{
    Task<FileValidationResult> ValidateFilesAsync(ProfileReadInfoDto profileInfo, string rootDirectory);
}
