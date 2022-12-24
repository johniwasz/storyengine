using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Repository.Phone
{



    /// <summary>
    /// Retrieves the type of a phone number. Determines if the phone number is a mobile number, landline, etc.
    /// </summary>
    public interface IPhoneInfoRetriever
    {

        Task<DataPhone> GetPhoneInfoAsync(string phoneNumber);


        Task<DataPhone> GetPhoneInfoAsync(Guid phoneId);


        Task SaveDatabasePhoneInfoAsync(DataPhone phoneInfo);
    }
}
