using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gml.Web.Api.Domains.System;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Storage;

namespace GmlCore.Interfaces.Procedures;

public interface ILauncherProcedures
{
    Task<string> CreateVersion(IVersionFile version, ILauncherBuild launcherBuild);
    Task Build(string version, string[] osNameVersions);
    IObservable<string> BuildLogs { get; }
    bool CanCompile(string version, out string message);
    Task<IEnumerable<string>> GetPlatforms();
}
