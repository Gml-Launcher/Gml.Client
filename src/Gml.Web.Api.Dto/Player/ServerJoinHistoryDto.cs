using System;

namespace Gml.Web.Api.Dto.Player;

public record ServerJoinHistoryDto
{
    public string ServerUuid { get; set; }
    public DateTime Date { get; set; }
}
