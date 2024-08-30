using System.Collections.Generic;
using Gml.Web.Api.Domains.Sentry;

namespace Gml.Web.Api.Dto.Sentry;

public class BaseSentryError
{
    public IEnumerable<SentryBugs> Bugs { get; set; }
    public long CountUsers { get; set; }
    public long Count { get; set; }
}
