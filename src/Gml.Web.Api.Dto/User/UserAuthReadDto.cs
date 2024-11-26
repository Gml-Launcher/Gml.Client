namespace Gml.Web.Api.Dto.User;

public class UserAuthReadDto
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string AccessToken { get; set; }
}
