using Newtonsoft.Json;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class ProfileProperties
{
    [JsonProperty("name")] public string Name { get; } = "textures";

    [JsonProperty("value")] public string Value { get; set; }

    [JsonProperty("signature")]
    public string Signature { get; set; } =
        "Cg=="; //Не используется, потому что это используется с подписью(сертификаты)
}
