using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Models
{
    public interface INaturalLanguageProcessor
    {

        Task<IntentRequest> ProcessTextMessageAsync(string message);

    }
}
