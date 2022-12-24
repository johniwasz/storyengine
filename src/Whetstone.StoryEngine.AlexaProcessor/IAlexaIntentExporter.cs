using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public interface IAlexaIntentExporter
    {



        Task<InteractionModel> GetIntentModelAsync(StoryTitle title, string locale);



    }
}
