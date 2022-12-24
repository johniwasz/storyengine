using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Data
{
    public abstract class SessionLoggerBase
    {


        public SessionLoggerBase()
        {

        }

        protected abstract ILogger Logger
        {
            get;
        }






    }
}
