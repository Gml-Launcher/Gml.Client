using GmlCore.Interfaces.Storage;

namespace Gml.Web.Api.Domains.Launcher;

public struct LauncherVersion : IVersionFile
{
    public string Version { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Guid { get; set; }
    public object Clone()
    {
        return new LauncherVersion
        {
            Version = Version,
            Title = Title,
            Description = Description,
            Guid = Guid,
        };
    }
}
