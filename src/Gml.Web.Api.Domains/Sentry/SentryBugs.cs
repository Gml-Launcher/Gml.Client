﻿using System.Collections.Generic;

namespace Gml.Web.Api.Domains.Sentry;

public class SentryBugs
{
    public string Exception { get; set; }
    public long CountUsers { get; set; }
    public long Count { get; set; }
    public IEnumerable<SentryGraphic> Graphics { get; set; }
}
