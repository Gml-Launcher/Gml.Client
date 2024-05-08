namespace Gml.Web.Api.Dto.Profile;

public class ProfileCreateInfoDto
{
    public string UserName { get; set; } = null!;
    public string ProfileName { get; set; } = null!;
    public string UserAccessToken { get; set; } = null!;
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public string GameAddress { get; set; }
    public int GamePort { get; set; }
    public bool IsFullScreen { get; set; }
    public int RamSize { get; set; }
    public string UserUuid { get; set; } = null!;
    public string OsType { get; set; }
    public string OsArchitecture { get; set; } = null!;
}
