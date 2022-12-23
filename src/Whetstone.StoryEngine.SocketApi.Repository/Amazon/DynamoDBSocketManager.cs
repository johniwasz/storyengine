using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.SocketApi.Repository.Amazon
{
    public class DynamoDBSocketManager : ISocketConnectionManager
    {
        private ILogger<DynamoDBSocketManager> _logger;

        public readonly string ConnectionIdField = "connectionId";
        public readonly string UserIdField = "userId";
        public readonly string ClientIdField = "clientId";
        public readonly string AuthTokenField = "authToken";
        public readonly string ExpirationTimeField = "expirationTime";
        public readonly string IsLocalField = "isLocal";

        /// <summary>
        /// DynamoDB table used to store the open connection ids. More advanced use cases could store logged on user map to their connection id to implement direct message chatting.
        /// </summary>
        private string ConnectionMappingTable { get; }

        /// <summary>
        /// DynamoDB service client used to store and retieve connection information from the ConnectionMappingTable
        /// </summary>
        private IAmazonDynamoDB DDBClient { get; }

        protected bool _isLocal = false;

        public DynamoDBSocketManager( ILogger<DynamoDBSocketManager> logger, IAmazonDynamoDB ddbClient, IOptions<SocketConfig> socketConfig )
        {
            _logger = logger;

            DDBClient = ddbClient;

            // Grab the name of the DynamoDB table from the configuration settings
            ConnectionMappingTable = socketConfig.Value.SocketConnectionTableName;
        }

        public bool IsLocal
        {
            get { return _isLocal;  }
        }

        public virtual async Task AddSocketAsync(IAuthenticatedSocket socket)
        {
            // Expiration time is now UTC plus 5 minutes
            long socketTTL = SocketExpirationHelper.GetNewSocketTTL();
            var ddbRequest = new PutItemRequest
            {
                TableName = ConnectionMappingTable,
                Item = new Dictionary<string, AttributeValue>
                    {
                        {UserIdField, new AttributeValue{ S = socket.UserId } },
                        {ConnectionIdField, new AttributeValue{ S = socket.Id}},
                        {ClientIdField, new AttributeValue{ S = socket.ClientId}},
                        {AuthTokenField, new AttributeValue{ S = socket.AuthToken}},
                        {ExpirationTimeField, new AttributeValue{ N = $"{socketTTL}" } },
                        {IsLocalField, new AttributeValue{ BOOL = this._isLocal } }
                    }
            };

            _logger.LogDebug("Putting new connection into DynamoDB");

            await DDBClient.PutItemAsync(ddbRequest);
        }

        public virtual async Task<ICollection<IAuthenticatedSocket>> GetSocketsForUserIdAsync( string userId )
        {
            List<IAuthenticatedSocket> lstSockets = new List<IAuthenticatedSocket>();

            // Filter by user id and expiration time of now
            long socketExpiration = SocketExpirationHelper.GetSocketExpirationNow();
            Dictionary<string, AttributeValue> expressionAttrValues = new Dictionary<string, AttributeValue>
            {
                {":v_UId", new AttributeValue { S = userId } },
                {":v_ExpTime", new AttributeValue { N = $"{socketExpiration}" } },
                {":v_IsLocal", new AttributeValue { BOOL = this._isLocal } }
            };

            // Get current connections for the user that have not expired
            var queryRequest = new QueryRequest
            {
                TableName = ConnectionMappingTable,
                KeyConditionExpression = $"{UserIdField} = :v_UId",
                FilterExpression = $"{ExpirationTimeField} > :v_ExpTime AND {IsLocalField} = :v_IsLocal",
                ExpressionAttributeValues = expressionAttrValues,
                ProjectionExpression = $"{UserIdField},{ConnectionIdField},{ClientIdField},{AuthTokenField}"
            };

            var queryResponse = await DDBClient.QueryAsync(queryRequest);

            _logger.LogDebug($"GetSocketsForUserId: {userId} returned {queryResponse.Items.Count} connections.");

            // Loop through all of the connections, create an authsocket and add to the list
            foreach (var item in queryResponse.Items)
            {
                // These three values are needed to build an authToken
                string uId = item[UserIdField].S;
                string itemConnectionId = item[ConnectionIdField].S;
                string clientId = item[ClientIdField].S;
                string authToken = item[AuthTokenField].S;

                IAuthenticatedSocket authSocket = new AuthSocketBase(itemConnectionId, uId, authToken, clientId);
                lstSockets.Add(authSocket);
            }

            return lstSockets;
        }


        public virtual async Task<IAuthenticatedSocket> GetSocketByIdAsync(string userId, string connectionId)
        {
            IAuthenticatedSocket authSocket = null;

            _logger.LogDebug("Getting connection from DynamoDB");

            var ddbRequest = new GetItemRequest
            {
                TableName = ConnectionMappingTable,
                Key = new Dictionary<string, AttributeValue>
                    {
                        {UserIdField, new AttributeValue { S = userId } },
                        {ConnectionIdField, new AttributeValue {S = connectionId}}
                    },
                ProjectionExpression = $"{UserIdField},{ConnectionIdField},{ClientIdField},{AuthTokenField}"
            };

            GetItemResponse itemResponse = await DDBClient.GetItemAsync(ddbRequest);

            if ( itemResponse.Item != null )
            {
                _logger.LogDebug($"Retrieved connection {connectionId} in DynamoDB");
                _logger.LogDebug($"Connection contains: {itemResponse.Item.Keys.Count} keys.");
                
                string authToken = itemResponse.Item[AuthTokenField].S;
                string clientId = itemResponse.Item[ClientIdField].S;

                authSocket = new AuthSocketBase(connectionId, userId, authToken, clientId );
            }
            else
            {
                _logger.LogDebug($"Unable to find connection {connectionId} in DynamoDB");
            }

            return authSocket;
        }

        public virtual async Task RemoveSocketAsync(string userId, string connectionId)
        {
            _logger.LogDebug("Removing connection from DynamoDB");

            var ddbRequest = new DeleteItemRequest
            {
                TableName = ConnectionMappingTable,
                Key = new Dictionary<string, AttributeValue>
                    {
                        {UserIdField, new AttributeValue { S = userId } },
                        {ConnectionIdField, new AttributeValue {S = connectionId}}
                    }
            };

            await DDBClient.DeleteItemAsync(ddbRequest);
        }

        public virtual async Task UpdateSocketTTL( string userId, string connectionId)
        {
            _logger.LogDebug($"Updating Socket TTL in DynamoDB, userId: {userId}, connectionId: {connectionId}");

            // Get the new TTL value for the socket
            long socketExpiration = SocketExpirationHelper.GetNewSocketTTL();
            Dictionary<string, AttributeValue> expressionAttrValues = new Dictionary<string, AttributeValue>
            {
                {":v_ExpTime", new AttributeValue { N = $"{socketExpiration}" } }
            };

            var ddbRequest = new UpdateItemRequest
            {
                TableName = ConnectionMappingTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserIdField, new AttributeValue { S = userId } },
                    { ConnectionIdField, new AttributeValue { S = connectionId } }
                },
                UpdateExpression = $"SET {ExpirationTimeField} = :v_ExpTime",
                ExpressionAttributeValues = expressionAttrValues
            };

            await DDBClient.UpdateItemAsync(ddbRequest);

        }

        public virtual async Task<ICollection<IAuthenticatedSocket>> GetSocketsForClientIdAsync(string userId, string clientId)
        {
            List<IAuthenticatedSocket> lstSockets = new List<IAuthenticatedSocket>();

            // Filter by user id and expiration time of now
            long socketExpiration = SocketExpirationHelper.GetSocketExpirationNow();
            Dictionary<string, AttributeValue> expressionAttrValues = new Dictionary<string, AttributeValue>
            {
                {":v_UId", new AttributeValue { S = userId } },
                {":v_ClientId", new AttributeValue { S = clientId } },
                {":v_ExpTime", new AttributeValue { N = $"{socketExpiration}" } },
                {":v_IsLocal", new AttributeValue { BOOL = this._isLocal } }
            };

            // Get current connections for the user that have not expired
            var queryRequest = new QueryRequest
            {
                TableName = ConnectionMappingTable,
                KeyConditionExpression = $"{UserIdField} = :v_UId",
                FilterExpression = $"{ExpirationTimeField} > :v_ExpTime AND {IsLocalField} = :v_IsLocal AND {ClientIdField} = :v_ClientId",
                ExpressionAttributeValues = expressionAttrValues,
                ProjectionExpression = $"{UserIdField},{ConnectionIdField},{ClientIdField},{AuthTokenField}"
            };

            var queryResponse = await DDBClient.QueryAsync(queryRequest);

            _logger.LogDebug($"GetSocketForClientIdAsync: {userId} returned {queryResponse.Items.Count} connections.");

            // Generally, there should be only one socket, but since sockets can get torn out from underneath us,
            // we may not have been informed that the socket was disconnected and end up with >1 socket for a unique client id
            // Return them all and let the write code remove any old sockets.

            foreach (var item in queryResponse.Items)
            {
                // These three values are needed to build an authToken
                string uId = item[UserIdField].S;
                string itemConnectionId = item[ConnectionIdField].S;
                string cId = item[ClientIdField].S;
                string authToken = item[AuthTokenField].S;

                IAuthenticatedSocket authSocket = new AuthSocketBase(itemConnectionId, uId, authToken, cId);
                lstSockets.Add(authSocket);
            }

            return lstSockets;
        }

    }
}
