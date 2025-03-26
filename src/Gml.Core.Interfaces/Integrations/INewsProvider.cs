using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.News;

namespace GmlCore.Interfaces.Integrations;

public interface INewsProvider
{
    Task<IReadOnlyCollection<INewsData>> GetNews(int count = 20);
    NewsListenerType Type { get; }
    string Name { get; }
    string Url { get; set; }
}
