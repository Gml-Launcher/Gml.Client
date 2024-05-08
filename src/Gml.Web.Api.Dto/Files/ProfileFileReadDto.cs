namespace Gml.Web.Api.Dto.Files;

public class ProfileFileReadDto
{
    public string Name { get; set; }
    public string Directory { get; set; }
    public long Size { get; set; }
    public string Hash { get; set; }
}
