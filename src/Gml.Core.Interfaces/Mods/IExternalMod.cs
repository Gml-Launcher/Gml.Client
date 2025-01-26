using System.Collections.Generic;

namespace GmlCore.Interfaces.Mods;

public interface IExternalMod : IMod
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string IconUrl { get; set; }
    public int DownloadCount { get; set; }
    public int FollowsCount { get; set; }
    IReadOnlyCollection<string> Files { get; set; }
    IReadOnlyCollection<IMod> Dependencies { get; set; }
}
