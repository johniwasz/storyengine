using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Models
{
    public interface INaturalLanguageProcessor
    {

        Task<IntentRequest> ProcessTextMessageAsync(string message);

    }
}
