using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Repository
{
    public interface IAuditLogRepository
    {


        Task<EngineSession> GetEngineSession(string environment, Guid engineRequestId);




    }
}
