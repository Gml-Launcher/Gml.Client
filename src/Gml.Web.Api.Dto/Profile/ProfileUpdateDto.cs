namespace Gml.Web.Api.Dto.Profile;

public class ProfileUpdateDto
{
    public string OriginalName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IconBase64 { get; set; } = null!;
    public string BackgroundImageKey { get; set; }
}
