using Newtonsoft.Json;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class Textures
{
    [JsonProperty("SKIN")]
    public SkinCape Skin { get; set; }
    [JsonProperty("CAPE")]
    public SkinCape Cape { get; set; }
}
