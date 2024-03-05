using System.Diagnostics;
using System.Text;

namespace Gml.Client.Tests;

public class ProcessUtil
{
    public event EventHandler<string>? OutputReceived;

    public event EventHandler? Exited;

    public Process Process { get; private set; }

    public ProcessUtil(Process process) => Process = process;

    public void StartWithEvents()
    {
        Process.StartInfo.CreateNoWindow = true;
        Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        Process.StartInfo.UseShellExecute = false;
        Process.StartInfo.RedirectStandardError = true;
        Process.StartInfo.RedirectStandardOutput = true;
        Process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        Process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        Process.EnableRaisingEvents = true;
        Process.ErrorDataReceived += (DataReceivedEventHandler) ((s, e) =>
        {
            if (OutputReceived == null)
                return;
            OutputReceived(this, e.Data ?? "");
        });

        Process.OutputDataReceived += (DataReceivedEventHandler) ((s, e) =>
        {
            if (OutputReceived == null)
                return;
            OutputReceived(this, e.Data ?? "");
        });

        Process.Exited += (s, e) =>
        {
            if (Exited == null)
                return;
            Exited(this, EventArgs.Empty);
        };

        Process.Start();
        Process.BeginErrorReadLine();
        Process.BeginOutputReadLine();
    }

    public Task WaitForExitTaskAsync() => Task.Run((Action) (() => this.Process.WaitForExit()));
}
