using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Data
{
    public interface ISessionReportingRepository
    {

        Task<EngineSession> GetSessionAsync(string sessionId);


        /// <summary>
        /// The date range applies to the start date of the session.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<List<EngineSession>> GetSessionDateRangeAsync(string titleId, DateTime? startDate, DateTime? endDate, StoryRequestType? requestType);



        Task<List<EngineSession>> GetSessionDateRangeAsync(string titleId, DateTime? startDate, DateTime? endDate);
    }
}
