using Whetstone.Alexa;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public interface IAlexaIntentExporter
    {



        Task<InteractionModel> GetIntentModelAsync(StoryTitle title, string locale);



    }
}
