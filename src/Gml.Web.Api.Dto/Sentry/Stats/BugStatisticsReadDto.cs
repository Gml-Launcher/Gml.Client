namespace Gml.Web.Api.Dto.Sentry.Stats;

public class BugStatisticsReadDto
{
    public int TotalBugs { get; set; }
    public int BugsThisMonth { get; set; }
    public double PercentageChangeMonth { get; set; }
    public int BugsToday { get; set; }
    public double PercentageChangeDay { get; set; }
    public int FixBugs { get; set; }
    public double PercentageChangeDayFixBugs { get; set; }
}
