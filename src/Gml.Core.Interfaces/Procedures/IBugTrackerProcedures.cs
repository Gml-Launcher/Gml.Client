using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;

namespace GmlCore.Interfaces.Procedures;

public interface IBugTrackerProcedures
{
    void CaptureException(IBugInfo bugInfo);
    Task<IEnumerable<IBugInfo>> GetAllBugs();
    Task<IBugInfo> GetBugId(string id);
}
