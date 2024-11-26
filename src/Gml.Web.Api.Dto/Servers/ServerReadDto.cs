namespace Gml.Web.Api.Dto.Servers;

public class ServerReadDto
{
    public string Name { get; set; }
    public string Version { get; set; }
    public bool IsOnline { get; set; }
    public int? Online { get; set; }
    public int? MaxOnline { get; set; }
}
