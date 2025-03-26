using GmlCore.Interfaces.Enums;

namespace Gml.Web.Api.Dto.News;

public class NewsGetListenerDto
{
    public string Url { get; set; }
    public NewsListenerType Type { get; set; }
}
