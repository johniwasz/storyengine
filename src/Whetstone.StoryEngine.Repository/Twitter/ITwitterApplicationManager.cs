using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Repository.Twitter
{
    public interface ITwitterApplicationManager
    {

        Task<AddTwitterApplicationResponse> AddTwitterApplicationAsync(AddTwitterApplicationRequest request);

    }
}
