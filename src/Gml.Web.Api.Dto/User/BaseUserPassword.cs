using Newtonsoft.Json;

namespace Gml.Web.Api.Dto.User;

public class BaseUserPassword
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string AccessToken { get; set; }
    [JsonProperty("2FACode")]
    public string TwoFactorCode { get; set; }
}
