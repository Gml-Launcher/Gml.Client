using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class Metadata
{
    [JsonPropertyName("id")] public string ServerName { get; set; } = "Gml.Server";

    [JsonPropertyName("implementationName")] public string ImplementationName { get; set; } = "Gml.Launcher";

    [JsonPropertyName("implementationVersion")]
    public string ImplementationVersion { get; set; } = "0.0.1";

    [JsonPropertyName("feature.no_mojang_namespace")]
    public bool NoMojang { get; set; } = true;

    [JsonPropertyName("feature.privileges_api")]
    public bool PrivilegesApi { get; set; } = true;
}
