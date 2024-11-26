using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;

namespace Gml.Web.Api.Dto.Sentry;

public class SentryOperationSystem
{
    public long Count { get; set; }
    public string OsType { get; set; }
}
