using System.Collections.Generic;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public interface ITwilioVerifier
    {

        Task<bool> ValidateTwilioMessageAsync(string path, IDictionary<string, string> headerValues, IDictionary<string, string> formVals, string alias);




    }
}
