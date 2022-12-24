using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class EFLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
            // do nothing
        }

        public ILogger CreateLogger(string categoryName)
        {

            return new EFLogger(categoryName);
        }
    }
}
