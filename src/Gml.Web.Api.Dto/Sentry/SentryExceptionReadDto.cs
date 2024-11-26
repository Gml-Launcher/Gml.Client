using System.Collections.Generic;
using Gml.Web.Api.Domains.Sentry;
using GmlCore.Interfaces.Launcher;

namespace Gml.Web.Api.Dto.Sentry;

public class SentryExceptionReadDto
{
    public string Exception { get; set; }
    public long CountUsers { get; set; }
    public long Count { get; set; }
    public IEnumerable<SentryGraphic> Graphic { get; set; }
    public IEnumerable<SentryOperationSystem> OperationSystems { get; set; }
    public IBugInfo BugInfo { get; set; }
    public string StackTrace { get; set; }
}
