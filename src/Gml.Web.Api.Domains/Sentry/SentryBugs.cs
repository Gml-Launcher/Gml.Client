using System.Collections.Generic;
using GmlCore.Interfaces.Launcher;

namespace Gml.Web.Api.Domains.Sentry;

public class SentryBugs
{
    public string Exception { get; set; }
    public long Users { get; set; }
    public long Errors { get; set; }
    public IEnumerable<IBugInfo> Bugs { get; set; }
}
