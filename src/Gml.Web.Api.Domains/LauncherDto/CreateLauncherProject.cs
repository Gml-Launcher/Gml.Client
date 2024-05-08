namespace Gml.Web.Api.Domains.LauncherDto;

public class CreateLauncherProject
{
    public string GitHubVersions { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Host { get; set; }
    public string SecretKey { get; set; }
    public string Version { get; set; }
    public string[] SkinDomains { get; set; }
}
