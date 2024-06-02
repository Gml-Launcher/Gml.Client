using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class Textures
{
    [JsonPropertyName("SKIN")] public SkinCape Skin { get; set; }

    [JsonPropertyName("CAPE")] public SkinCape Cape { get; set; }
}
