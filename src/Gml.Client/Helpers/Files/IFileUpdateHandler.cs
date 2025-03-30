using System.Diagnostics;
using Gml.Web.Api.Dto.Profile;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gml.Client.Helpers.Files;

public interface IFileUpdateHandler
{
    Task<FileValidationResult> ValidateFilesAsync(ProfileReadInfoDto profileInfo, string rootDirectory);
}
