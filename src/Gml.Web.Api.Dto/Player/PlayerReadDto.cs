using System;

namespace Gml.Web.Api.Dto.Player;

public class PlayerReadDto : PlayerTextureDto
{
    public string Uuid { get; set; }
    public string Name { get; set; } = null!;
    public string AccessToken { get; set; }
    public DateTime ExpiredDate { get; set; }
}
