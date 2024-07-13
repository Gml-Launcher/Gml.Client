using System;

namespace GmlCore.Interfaces.Launcher;

public interface ILauncherBuild
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string ExecutableFilePath { get; set; }
    public DateTime DateTime { get; set; }
}
