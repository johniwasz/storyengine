using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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
