using Gml.Web.Api.Dto.Files;

namespace Gml.Client.Helpers.Files;

public class FileValidationResult
{
    public IEnumerable<ProfileFileReadDto> FilesToUpdate { get; set; } = [];
    public IEnumerable<ProfileFileReadDto> FilesToDelete { get; set; } = [];
}
