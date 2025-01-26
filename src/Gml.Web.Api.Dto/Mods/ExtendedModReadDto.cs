namespace Gml.Web.Api.Dto.Mods;

public class ExtendedModReadDto : ModReadDto
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string IconUrl { get; set; }
    public int DownloadCount { get; set; }
    public int FollowsCount { get; set; }
}
