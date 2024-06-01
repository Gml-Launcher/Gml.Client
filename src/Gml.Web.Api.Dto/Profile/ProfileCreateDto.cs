using GmlCore.Interfaces.Enums;
using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Profile;

public class ProfileCreateDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GameLoader GameLoader { get; set; }

    public string IconBase64 { get; set; } = null!;
}
