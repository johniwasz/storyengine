using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine
{

    public delegate Task<TResult> ProcessThrottledRequestAsync<T, TResult>(T request, CancellationToken token);


    public delegate Task ProcessThrottledRequestAsync<T>(T request, CancellationToken token);


    public static class ThrottleManager
    {

       public static async Task<TResponse> ThrottleRequestWithRetries<T, TResponse>(T requestItem, ProcessThrottledRequestAsync<T, TResponse> retriever, int engineTimeout, int maxRetries, ILogger logger, CancellationToken token = default)
        {
            int retryCount = 0;

            if (requestItem == null)
                throw new ArgumentNullException(nameof(requestItem));

            if (retriever == null)
                throw new ArgumentNullException(nameof(retriever));

            if (maxRetries < 0)
                throw new ArgumentException($"{nameof(maxRetries)} cannot be less than zero (0)");

            if (engineTimeout < 0)
                throw new ArgumentException($"{nameof(engineTimeout)} cannot be less than zero (0)");


            TResponse resp = default(TResponse);

            // If we haven't gotten a response and there are retries
            // remaining, then keep going.
            while (resp == null && retryCount < maxRetries)
            {
                Stopwatch getItemTimer = new Stopwatch();
                try
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();

                    CancellationToken cancelToken = token == default ? tokenSource.Token : token;
                    getItemTimer.Start();

                    // Create a task to call the throttled request
                    Task<TResponse> getTask = retriever(requestItem, cancelToken);


                    // Create a competing delay task that runs for the duration of the engine timeout.
                    // The winner is the task that finishes first. 
                    Task winner = await Task.WhenAny(getTask, Task.Delay(engineTimeout, cancelToken));

                    // Cancel the losing task.
                    tokenSource.Cancel();

                    // If the throttled request completed first, then get the return value. If the delay wins first
                    // then the throttled task timed out. The resp will be null and the retry count will be incremented.
                    if (winner == getTask)
                    {
                        if (getTask.IsCompletedSuccessfully)
                        {
                            // Pull the response from the task
                            resp = getTask.Result;
                        }
                        else
                        {

                           
                            // if it threw an unhandled exception, then rethrow 
                            if (getTask.Exception == null)
                            {
                                string errorText = "Throttled task did not complete successfully.";
                                AWSXRayRecorder.Instance.AddException(new Exception(errorText));
                                // the task did not complete successfully, but did not raise an unhandled exception. Log it. Do not throw.
                                logger.LogError(errorText);
                            }
                            else
                            {
                                // the task did not complete successfully and raised an unexpected unhandled exception. throw the exception.
                                if (getTask.Exception.InnerExceptions.Count == 1)
                                {
                                    Exception ex = getTask.Exception.InnerException;
                                    AWSXRayRecorder.Instance.AddException(ex);
                                    throw ex;
                                }
                                else
                                {
                                    Exception ex = getTask.Exception;
                                    AWSXRayRecorder.Instance.AddException(ex);
                                    throw getTask.Exception;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error throttling request");
                }
                finally
                {
                    getItemTimer.Stop();
                }

                // If the response is null, then increment the retry count and add a brief delay. The service being invoked could be 
                // throttled and a slight delay may help. The delay is incremented based on the retry count (1st retry is 100ms, 2nd is 200ms, etc.)
                if (resp == null)
                {
                    retryCount++;
                    AWSXRayRecorder.Instance.MarkThrottle();
                    int millisecondDelay = retryCount * 100;
                    logger.LogWarning($"Throttled request is on retry #{retryCount} and waiting {millisecondDelay}ms");
                    await Task.Delay(millisecondDelay);
                }
            };

            return resp;
        }


        public static async Task ThrottleRequestWithRetries<T>(T requestItem, ProcessThrottledRequestAsync<T> retriever, int engineTimeout, int maxRetries, ILogger logger, CancellationToken token = default)
        {
            int retryCount = 0;

            if (requestItem == null)
                throw new ArgumentNullException(nameof(requestItem));

            if (retriever == null)
                throw new ArgumentNullException(nameof(retriever));

            if (maxRetries < 0)
                throw new ArgumentException($"{nameof(maxRetries)} cannot be less than zero (0)");

            if (engineTimeout < 0)
                throw new ArgumentException($"{nameof(engineTimeout)} cannot be less than zero (0)");

            bool isComplete = false;
            // If we haven't gotten a response and there are retries
            // remaining, then keep going.
            while (!isComplete && retryCount < maxRetries)
            {
                Stopwatch getItemTimer = new Stopwatch();
                try
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();

                    CancellationToken cancelToken = token == default ? tokenSource.Token : token;
                    getItemTimer.Start();

                    // Create a task to call the throttled request
                    Task getTask = retriever(requestItem, cancelToken);


                    // Create a competing delay task that runs for the duration of the engine timeout.
                    // The winner is the task that finishes first. 
                    Task winner = await Task.WhenAny(getTask, Task.Delay(engineTimeout, cancelToken));

                    // Cancel the losing task.
                    tokenSource.Cancel();

                    // If the throttled request completed first, then get the return value. If the delay wins first
                    // then the throttled task timed out. The resp will be null and the retry count will be incremented.
                    if (winner == getTask)
                    {
                        if (getTask.IsCompletedSuccessfully)
                        {
                         
                            isComplete = true;
                        }
                        else
                        {
                            // if it threw an unhandled exception, then rethrow 
                            if (getTask.Exception == null)
                            {

                                // the task did not complete successfully, but did not raise an unhandled exception. Log it. Do not throw.
                                logger.LogError(getTask.Exception, $"Task did not complete successfully.");
                            }
                            else
                            {
                                // the task did not complete successfully and raised an unexpected unhandled exception. throw the exception.
                                if (getTask.Exception.InnerExceptions.Count == 1)
                                {
                                    throw getTask.Exception.InnerException;
                                }
                                else
                                    throw getTask.Exception;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error throttling request");
                }
                finally
                {
                    getItemTimer.Stop();
                }

                // If the response is null, then increment the retry count and add a brief delay. The service being invoked could be 
                // throttled and a slight delay may help. The delay is incremented based on the retry count (1st retry is 100ms, 2nd is 200ms, etc.)
                if (!isComplete)
                {
                    retryCount++;

                    int millisecondDelay = retryCount * 100;
                    logger.LogWarning($"Throttled request is on retry #{retryCount} and waiting {millisecondDelay}ms");
                    await Task.Delay(millisecondDelay);
                }
            };

          
        }
    }
}
