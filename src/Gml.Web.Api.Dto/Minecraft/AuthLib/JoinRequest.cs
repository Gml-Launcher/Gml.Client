using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class JoinRequest
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; set; }

    [JsonPropertyName("selectedProfile")] public string SelectedProfile { get; set; }

    [JsonPropertyName("serverId")] public string ServerId { get; set; }
}
