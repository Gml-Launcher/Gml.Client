namespace Gml.Web.Api.Domains.User;

public class User : BaseUser
{
    public string Password { get; set; }
    public string AccessToken { get; set; }
    public string Email { get; set; }
}
