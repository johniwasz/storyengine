using Amazon.Lambda.Core;
using System.Diagnostics;

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
