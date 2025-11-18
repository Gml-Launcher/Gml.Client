namespace Gml.Client.Interfaces;

public interface ILauncherUser
{
    public string Name { get; set; }
    public string AccessToken { get; set; }
    public string Uuid { get; set; }
    public bool IsAuth { get; set; }
    public bool IsNotExpired { get; }
    public bool Has2Fa { get; }
    public DateTime ExpiredDate { get; set; }
    public string TextureUrl { get; set; }
}
