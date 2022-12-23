using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Story
{

    public class ClientAppMapping
    {

        public ClientAppMapping()
        {

        }

        public ClientAppMapping(Client clientType, List<string> clientAppIds)
        {
            ClientType = clientType;
            ClientAppIds = clientAppIds;

        }

        public Client ClientType { get; set; }

        public List<string> ClientAppIds { get; set; }
    }
}
