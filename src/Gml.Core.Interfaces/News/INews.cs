using GmlCore.Interfaces.Enums;

namespace GmlCore.Interfaces.News;

public class INews
{
    public string Url { get; set; }
    public NewsListenerType Type { get; set; }
}
