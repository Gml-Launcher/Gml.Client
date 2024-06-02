using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Minecraft.AuthLib;

public class Profile
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("properties")] public List<ProfileProperties> Properties { get; set; }
}
