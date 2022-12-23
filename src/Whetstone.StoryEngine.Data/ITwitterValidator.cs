using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Twitter;

namespace Whetstone.StoryEngine.Data
{
    public interface ITwitterValidator
    {
        Task<TwitterCrcResponse> GenerateTwitterCrcResponseAsync(Guid organizationid, Guid credentialId, string twitterCrc);
    }
}
