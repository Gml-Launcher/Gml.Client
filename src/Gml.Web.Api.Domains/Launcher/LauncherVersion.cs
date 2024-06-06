using System.IO;
using GmlCore.Interfaces.Storage;
using Microsoft.Extensions.Primitives;

namespace Gml.Web.Api.Domains.Launcher;

public class LauncherVersion : IVersionFile
{
    public string Version { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Stream? File { get; set; }
    public string Guid { get; set; }
}
