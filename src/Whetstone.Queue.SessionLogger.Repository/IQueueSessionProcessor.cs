using Amazon.Lambda.SQSEvents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Whetstone.Queue.SessionLogger.Repository
{
    public interface IQueueSessionProcessor
    {

        Task ProcessSessionLogMessages(List<SQSEvent.SQSMessage> sqsMessages);

    }
}
