using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class MetadataResponse
{
    [JsonPropertyName("meta")] public Metadata Meta { get; set; } = new();

    [JsonPropertyName("skinDomains")] public string[] SkinDomains { get; set; }

    [JsonPropertyName("signaturePublickey")] public string SignaturePublicKey { get; set; }
}
