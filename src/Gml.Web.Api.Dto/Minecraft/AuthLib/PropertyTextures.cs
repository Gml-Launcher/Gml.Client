using Newtonsoft.Json;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class PropertyTextures
{
    [JsonProperty("timestamp")] public long Timestamp { get; set; }

    [JsonProperty("profileId")] public string ProfileId { get; set; }

    [JsonProperty("profileName")] public string ProfileName { get; set; }

    [JsonProperty("textures")] public Textures Textures { get; set; }

    [JsonProperty("signatureRequired")] public bool SignatureRequired { get; set; }
}
