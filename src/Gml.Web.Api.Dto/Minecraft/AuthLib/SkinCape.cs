using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class SkinCape
{
    [JsonPropertyName("url")] public string Url { get; set; }
}
