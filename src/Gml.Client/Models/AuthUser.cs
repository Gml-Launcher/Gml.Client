namespace Gml.Client.Models;

public class AuthUser : IUser
{
    public string Name { get; set; }
    public string? AccessToken { get; set; }
    public string? Uuid { get; set; }
    public bool IsAuth { get; set; }
    public bool Has2Fa { get; set; }
    public DateTime ExpiredDate { get; set; }

    public bool IsNotExpired => ExpiredDate != DateTime.MinValue && ExpiredDate > DateTime.Now;
}
