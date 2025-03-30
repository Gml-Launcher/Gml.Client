using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.News;

namespace GmlCore.Interfaces.Integrations;

public interface INewsListenerProvider
{
    IReadOnlyCollection<INewsProvider> Providers { get; }
    Task<ICollection<INewsData>> GetNews(int count = 20);
    Task RefreshAsync(long nubmer = 0);
    Task AddListener(INewsProvider newsProvider);
    Task RemoveListener(INewsProvider newsProvider);
    Task Restore();
    Task RemoveListenerByType(NewsListenerType type);
}
