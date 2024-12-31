using GmlCore.Interfaces.User;

namespace Gml.Web.Api.Dto.Player;

public class PlayerTextureDto : IPlayerTexture
{
    public string TextureSkinUrl { get; set; } = null!;
    public string TextureCloakUrl { get; set; } = null!;
    public string TextureSkinGuid { get; set; } = null!;
    public string TextureCloakGuid { get; set; } = null!;
    public string FullSkinUrl { get; set; }
}
