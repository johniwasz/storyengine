using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Repository
{
    public interface IAuditLogRepository
    {


        Task<EngineSession> GetEngineSession(string environment, Guid engineRequestId);

        


    }
}
