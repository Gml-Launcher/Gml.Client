namespace Gml.Web.Api.Dto.Profile;

public class ProfileCreateDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string GameLoader { get; set; }
    public string IconBase64 { get; set; } = null!;
}
