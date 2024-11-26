using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Sentry;

namespace GmlCore.Interfaces.Procedures;

public interface IBugTrackerProcedures
{
    void CaptureException(IBugInfo bugInfo);
    IBugInfo CaptureException(Exception exception);
    Task<IEnumerable<IBugInfo>> GetAllBugs();
    Task<IBugInfo?> GetBugId(Guid id);
    Task<IEnumerable<IBugInfo>> GetFilteredBugs(Expression<Func<IStorageBug, bool>> filter);
}
