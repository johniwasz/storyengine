
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
