using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class DebugLogger : ILambdaLogger
    {
        public void Log(string message)
        {
            Debug.Write(message);
        }

        public void LogLine(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
