using System;

namespace Gml.Web.Api.Dto.Player;

public class PlayerReadDto
{
    public string Name { get; set; } = null!;
    public string AccessToken { get; set; }
    public string Uuid { get; set; }
    public DateTime ExpiredDate { get; set; }
    public string TextureUrl { get; set; }
}
