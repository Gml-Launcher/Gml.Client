using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class ProfileProperties
{
    [JsonPropertyName("name")] public string Name { get; } = "textures";

    [JsonPropertyName("value")] public string Value { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; } =
        "Cg=="; //Не используется, потому что это используется с подписью(сертификаты)
}
