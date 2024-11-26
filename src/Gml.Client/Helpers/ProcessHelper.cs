using System.Diagnostics;
using System.Reactive.Linq;

namespace Gml.Client.Helpers;

public static class ProcessHelper
{
    private static IDisposable? _watchDisposable;

    public static void StartWatch(this Process process)
    {
        _watchDisposable?.Dispose();

        _watchDisposable = process.Modules
            .Cast<ProcessModule>()
            .ToObservable()
            .Subscribe(module => Console.WriteLine($"Module added: {module}"),
                () => Console.WriteLine("A module was removed"));
    }
}
