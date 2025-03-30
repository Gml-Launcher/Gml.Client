using GmlCore.Interfaces.Enums;

namespace Gml.Web.Api.Dto.News;

public class NewsListenerDto
{
    public string Url { get; set; }
    public NewsListenerType Type { get; set; }
}
