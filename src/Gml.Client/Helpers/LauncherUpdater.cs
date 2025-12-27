using System;
using System.Diagnostics;
using Gml.Web.Api.Domains.System;

namespace Gml.Client.Helpers;

public class LauncherUpdater
{
    public static void FileReplaceAndRestart(OsType osType, string newFileName, string originalFileName)
    {
        Start(osType, newFileName, true);
    }

    private static void ExecuteCommand(string command, OsType osType, string? workingDirectory = null)
    {
        ProcessStartInfo processInfo;
        if (osType == OsType.Windows)
            processInfo = new ProcessStartInfo("cmd.exe", $"/C {command}")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        else
            processInfo = new ProcessStartInfo("bash", $"-c \"{command}\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        if (workingDirectory is not null) processInfo.WorkingDirectory = workingDirectory;
        using (var process = Process.Start(processInfo))
        {
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0) throw new Exception($"Command '{command}' failed with error: {error}");
        }
    }

    public static void Start(OsType osType, string fileName, bool skipUpdate = false)
    {
        var arguments = skipUpdate ? "-skip-update" : string.Empty;

        switch (osType)
        {
            case OsType.Linux:
            case OsType.OsX:
                ExecuteCommand($"chmod +x \"{fileName}\"", osType);
                ExecuteCommand($"./{fileName} {arguments}", osType);
                break;
            case OsType.Windows:
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                break;
            case OsType.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(osType), osType, null);
        }

        Environment.Exit(0);
    }
}
