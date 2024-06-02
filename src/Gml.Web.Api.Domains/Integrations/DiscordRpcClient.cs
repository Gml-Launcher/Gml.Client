using GmlCore.Interfaces.Integrations;

namespace Gml.Web.Api.Domains.Integrations;

public class DiscordRpcClient : IDiscordRpcClient
{
    public string ClientId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string LargeImageKey { get; set; } = string.Empty;
    public string LargeImageText { get; set; } = string.Empty;
    public string SmallImageKey { get; set; } = string.Empty;
    public string SmallImageText { get; set; } = string.Empty;
}
