namespace Gml.Client.Models;

public class AuthUser : IUser
{
    public string Name { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public DateTime ExpiredDate { get; set; }
    public string TextureUrl { get; set; } = string.Empty;
    public bool IsAuth { get; set; }
    public bool Has2Fa { get; set; }
    public bool IsNotExpired => ExpiredDate != DateTime.MinValue && ExpiredDate > DateTime.Now;
}
