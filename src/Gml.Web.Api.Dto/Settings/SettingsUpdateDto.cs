using GmlCore.Interfaces.Enums;

namespace Gml.Web.Api.Dto.Settings;

public class SettingsUpdateDto
{
    public bool RegistrationIsEnabled { get; set; }
    public int StorageType { get; set; }
    public string StorageHost { get; set; }
    public string StorageLogin { get; set; }
    public string CurseForgeKey { get; set; }
    public string VkKey { get; set; }
    public string StoragePassword { get; set; }
    public TextureProtocol TextureProtocol { get; set; }
}
