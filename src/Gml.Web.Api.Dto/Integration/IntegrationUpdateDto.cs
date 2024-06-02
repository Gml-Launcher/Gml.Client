using GmlCore.Interfaces.Enums;
using System.Text.Json.Serialization;

namespace Gml.Web.Api.Dto.Integration;

public class IntegrationUpdateDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuthType AuthType { get; set; }

    public string Endpoint { get; set; } = null!;
}
