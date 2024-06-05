using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gml.Web.Api.Dto.Integration;

public class IntegrationUpdateDto
{
    [JsonConverter(typeof(StringEnumConverter))]
    public AuthType AuthType { get; set; }

    public string Endpoint { get; set; } = null!;
}
