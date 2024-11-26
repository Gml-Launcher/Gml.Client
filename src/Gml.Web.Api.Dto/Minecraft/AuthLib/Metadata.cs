using Newtonsoft.Json;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class Metadata
{
    [JsonProperty("id")] public string ServerName { get; set; } = "Gml.Server";

    [JsonProperty("implementationName")] public string ImplementationName { get; set; } = "Gml.Launcher";

    [JsonProperty("implementationVersion")]
    public string ImplementationVersion { get; set; } = "0.0.1";

    [JsonProperty("feature.no_mojang_namespace")]
    public bool NoMojang { get; set; } = true;

    [JsonProperty("feature.privileges_api")]
    public bool PrivilegesApi { get; set; } = true;
}
