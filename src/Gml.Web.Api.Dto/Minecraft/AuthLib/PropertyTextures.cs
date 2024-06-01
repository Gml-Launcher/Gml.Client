using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class PropertyTextures
{
    [JsonPropertyName("timestamp")] public long Timestamp { get; set; }

    [JsonPropertyName("profileId")] public string ProfileId { get; set; }

    [JsonPropertyName("profileName")] public string ProfileName { get; set; }

    [JsonPropertyName("textures")] public Textures Textures { get; set; }

    [JsonPropertyName("signatureRequired")] public bool SignatureRequired { get; set; }
}
