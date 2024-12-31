using System.Collections.Generic;

namespace Gml.Web.Api.Dto.Player;

public class ExtendedPlayerReadDto : PlayerReadDto
{
    public bool IsBanned { get; set; }
    public List<AuthUserHistoryDto> AuthHistory { get; set; } = new();
    public List<ServerJoinHistoryDto> ServerJoinHistory { get; set; } = new();
}
