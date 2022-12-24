namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// This is used by the SessionQueueLogger to determine which queue to 
    /// send the session audit message to.
    /// </summary>
    public class SessionAuditConfig
    {
        private string _sessionAuditQueue;


        public SessionAuditConfig()
        {
            _sessionAuditQueue = null;
        }

        public SessionAuditConfig(string sessionQueue)
        {
            _sessionAuditQueue = sessionQueue;

        }

        public string SessionAuditQueue
        {
            get { return _sessionAuditQueue; }
            set { _sessionAuditQueue = value; }

        }


    }
}
