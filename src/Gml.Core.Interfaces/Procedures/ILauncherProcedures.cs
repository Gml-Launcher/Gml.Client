using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Interfaces.Procedures;

public interface ILauncherProcedures
{
    IObservable<string> BuildLogs { get; }
    Task<string> CreateVersion(IVersionFile version, ILauncherBuild launcherBuild);
    Task Build(string version, string[] osNameVersions);
    bool CanCompile(string version, out string message);
    Task<IEnumerable<string>> GetPlatforms();
}
