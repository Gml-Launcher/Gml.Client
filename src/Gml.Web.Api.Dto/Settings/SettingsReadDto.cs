namespace Gml.Web.Api.Dto.Settings;

public class SettingsReadDto
{
    public bool RegistrationIsEnabled { get; set; }
    public int StorageType { get; set; }
    public string StorageHost { get; set; }
    public string StorageLogin { get; set; }
}
