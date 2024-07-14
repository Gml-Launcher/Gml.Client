using System.Diagnostics;
using Gml.Web.Api.Domains.System;

namespace Gml.Client.Helpers;

public class LauncherUpdater
{
    public static void FileReplaceAndRestart(OsType osType, string newFileName, string originalFileName)
    {
        int currentProcessId = Process.GetCurrentProcess().Id;
        string scriptFileName = Path.GetTempFileName();
        string scriptContent = string.Empty;

        var newFileInfo = new FileInfo(newFileName);
        var originalFileInfo = new FileInfo(originalFileName);

        switch (osType)
        {
            case OsType.Linux:
            case OsType.OsX:
                scriptContent = $@"
#!/bin/bash
sleep 1
kill {currentProcessId}
while [ -e /proc/{currentProcessId} ]; do sleep 0.1; done
mv ""{newFileInfo.FullName}"" ""{originalFileInfo.FullName}""
nohup ""{originalFileInfo.FullName}"" &
rm ""{scriptFileName}""";
                File.WriteAllText(scriptFileName, scriptContent);
                ExecuteCommand($"chmod +x \"{scriptFileName}\"", osType);
                ExecuteCommand($"nohup \"{scriptFileName}\" &", osType, Path.GetDirectoryName(newFileInfo.FullName));
                break;

            case OsType.Windows:
                scriptContent = $@"
@echo off
timeout /t 1 /nobreak
taskkill /PID {currentProcessId} /F
:waitloop
timeout /t 0.1 >nul
tasklist /fi ""pid eq {currentProcessId}"" | find /i ""{currentProcessId}"" >nul
if not errorlevel 1 goto waitloop
move /Y ""{newFileInfo.FullName}"" ""{originalFileInfo.FullName}""
start """" ""{originalFileInfo.FullName}""
del ""{scriptFileName}.bat""";
                File.WriteAllText(scriptFileName + ".bat", scriptContent);
                ExecuteCommand($"\"{scriptFileName}.bat\"", osType, Path.GetDirectoryName(newFileInfo.FullName));
                break;

            default:
                throw new NotSupportedException("Unsupported OS type.");
        }

        Environment.Exit(0);
    }

    private static void ExecuteCommand(string command, OsType osType, string? workingDirectory = null)
    {
        ProcessStartInfo processInfo;

        if (osType == OsType.Windows)
        {
            processInfo = new ProcessStartInfo("cmd.exe", $"/C {command}")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        else
        {
            processInfo = new ProcessStartInfo("bash", $"-c \"{command}\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        if (workingDirectory is not null)
        {
            processInfo.WorkingDirectory = workingDirectory;
        }

        using (var process = Process.Start(processInfo))
        {
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command '{command}' failed with error: {error}");
            }
        }
    }

}
