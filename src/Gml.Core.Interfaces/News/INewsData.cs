using System;
using GmlCore.Interfaces.Enums;

namespace GmlCore.Interfaces.News;

public interface INewsData
{
    public string Title { get; }
    public string Content { get; }
    public DateTimeOffset Date { get; }
    NewsListenerType Type { get; set; }
}
