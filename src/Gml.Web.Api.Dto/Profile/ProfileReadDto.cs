using System;

namespace Gml.Web.Api.Dto.Profile;

public class ProfileReadDto
{
    public string Name { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string Description { get; set; }
    public string GameVersion { get; set; }
    public string LaunchVersion { get; set; }
    public string IconBase64 { get; set; }
    public string Background { get; set; }
}
