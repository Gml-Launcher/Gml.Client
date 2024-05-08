namespace Gml.Web.Api.Dto.Profile;

public class ProfileRestoreDto
{
    public string Name { get; set; } = null!;
    public string OsType { get; set; }
    public string OsArchitecture { get; set; } = null!;
}
