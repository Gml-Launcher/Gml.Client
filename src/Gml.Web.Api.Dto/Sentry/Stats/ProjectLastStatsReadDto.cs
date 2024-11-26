using System;

namespace Gml.Web.Api.Dto.Sentry.Stats;

public class ProjectLastStatsReadDto
{
    public DateTime Date { get; set; }
    public int Launcher { get; set; }
    public int Backend { get; set; }
}
