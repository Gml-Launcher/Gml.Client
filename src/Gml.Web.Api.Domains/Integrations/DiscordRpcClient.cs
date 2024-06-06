using GmlCore.Interfaces.Integrations;

namespace Gml.Web.Api.Domains.Integrations;

public class DiscordRpcClient : IDiscordRpcClient
{
    public string ClientId { get; set; }
    public string Details { get; set; }
    public string LargeImageKey { get; set; }
    public string LargeImageText { get; set; }
    public string SmallImageKey { get; set; }
    public string SmallImageText { get; set; }
}
