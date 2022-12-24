using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using LogLevel = Amazon.Lambda.Core.LogLevel;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class EFLogger : ILogger
    {

        private readonly string _category;
        public EFLogger(string category)
        {
            _category = category;

        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return new EFLoggerScope();

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                string logLevelText = logLevel.ToString();

                StringBuilder logMessage = new StringBuilder();

                logMessage.Append(logLevelText).Append(':');

                if (!string.IsNullOrWhiteSpace(_category))
                    logMessage.Append(_category).Append(':');
                // The default message formatter skips over the exceptions

                try
                {
                    string formattedMessage = formatter(state, exception);
                    logMessage.Append(formattedMessage);


                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                }


                if (exception != null)
                {
                    logMessage.AppendLine();
                    logMessage.Append(GetFullExceptionText(exception));
                    logMessage.AppendLine();
                }


                string logText = logMessage.ToString();

                //  Debug.WriteLine(logText);
                LambdaLogger.Log(logText);


#if DEBUG
                Debug.WriteLine(logText);
#endif
            }




        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }

        private string GetFullExceptionText(Exception ex)
        {
            StringBuilder exText = new StringBuilder();

            if (ex is AggregateException)
            {
                exText.AppendLine("AggregateException: ");
                AggregateException aggEx = ex as AggregateException;

                foreach (Exception foundEx in aggEx.InnerExceptions)
                {
                    exText.AppendLine(GetFullExceptionText(foundEx));
                }

            }
            else
            {
                exText.AppendLine(ex.ToString());


                if (ex.InnerException != null)
                {
                    exText.Append("   ");
                    exText.Append(GetFullExceptionText(ex.InnerException));
                    exText.AppendLine();
                }

            }


            return exText.ToString();
        }
    }

    public class EFLoggerScope : IDisposable
    {
        public void Dispose()
        {
            // do nothing
        }
    }
}
