using GmlCore.Interfaces.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gml.Web.Api.Dto.Profile;

public class ProfileCreateDto
{
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string LoaderVersion { get; set; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public GameLoader GameLoader { get; set; }

    public string IconBase64 { get; set; } = null!;
}
