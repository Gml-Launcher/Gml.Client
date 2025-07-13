namespace Gml.Web.Api.Dto.Profile;

public class ProfileUpdateDto
{
    public string OriginalName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string Description { get; set; } = null!;
    public string IconBase64 { get; set; } = null!;
    public string BackgroundImageKey { get; set; }
    public string JvmArguments { get; set; }
    public string GameArguments { get; set; }
    public int RecommendedRam { get; set; }
}
