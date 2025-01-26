namespace GmlCore.Interfaces.User;

public interface IPlayerTexture
{
    string TextureSkinUrl { get; set; }
    string TextureCloakUrl { get; set; }
    string TextureSkinGuid { get; set; }
    string TextureCloakGuid { get; set; }
    string? FullSkinUrl { get; set; }
}
