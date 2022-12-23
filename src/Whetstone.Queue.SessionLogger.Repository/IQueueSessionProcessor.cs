using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace Whetstone.Queue.SessionLogger.Repository
{
    public interface IQueueSessionProcessor
    {

       Task ProcessSessionLogMessages(List<SQSEvent.SQSMessage> sqsMessages);

    }
}
