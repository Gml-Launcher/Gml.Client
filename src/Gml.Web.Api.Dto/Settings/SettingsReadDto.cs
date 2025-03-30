namespace Gml.Web.Api.Dto.Settings;

public class SettingsReadDto
{
    public bool RegistrationIsEnabled { get; set; }
    public int StorageType { get; set; }
    public string StorageHost { get; set; }
    public string CurseForgeKey { get; set; }
    public string VkKey { get; set; }
    public string StorageLogin { get; set; }
    public int TextureProtocol { get; set; }
}
