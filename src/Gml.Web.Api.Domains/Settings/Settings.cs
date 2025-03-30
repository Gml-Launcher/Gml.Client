using GmlCore.Interfaces.Enums;

namespace Gml.Web.Api.Domains.Settings;

public class Settings
{
    public int Id { get; set; }
    public string StorageHost { get; set; }
    public string StorageLogin { get; set; }
    public string StoragePassword { get; set; }
    public string CurseForgeKey { get; set; }
    public string VkKey { get; set; }
    public StorageType StorageType { get; set; }
    public TextureProtocol TextureProtocol { get; set; }
    public bool RegistrationIsEnabled { get; set; }
}
