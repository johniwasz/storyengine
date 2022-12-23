using System;
using Xunit;

using Whetstone.UnitTests;
using Whetstone.StoryEngine.SocketApi.Repository;

using Microsoft.Extensions.DependencyInjection;

namespace Whetstone.UnitTests
{
    public class SocketTests
    {
        [Fact]
        public async void TestSocketHandler()
        {
            MockFactory mockFactory = new MockFactory();

            IServiceCollection servColl = mockFactory.InitServiceCollection();

            IServiceProvider services = servColl.BuildServiceProvider();

            ISocketConnectionManager connMgr = services.GetService<ISocketConnectionManager>();
            ISocketHandler socketHandler = services.GetService<ISocketHandler>();
            ISocketMessageSender sender = services.GetService<ISocketMessageSender>();

            string connectionId = Guid.NewGuid().ToString();
            string userId = Guid.NewGuid().ToString();
            string authToken = userId;
            string clientId = Guid.NewGuid().ToString();

            IAuthenticatedSocket authSocket = new AuthSocketBase(connectionId, userId, authToken, clientId);

            await socketHandler.OnConnect(authSocket);
            IAuthenticatedSocket connSocket = await connMgr.GetSocketByIdAsync(userId, connectionId);

            Assert.NotNull(connSocket);
            Assert.Same(authSocket, connSocket);

            long clientMsgId = 1L;


            string message = String.Format("{{ \"message\": \"sendMessage\", \"authToken\": \"{0}\", \"clientMsgId\": {1}, \"data\": \"Hello Weird!\" }}", authToken, clientMsgId );

            ISocketResponse response = await socketHandler.OnReceiveMessage(userId, connectionId, message);

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

            message = String.Format("{{ \"message\": \"bogusMessage\", \"authToken\": \"{0}\", \"clientMsgId\": {1}, \"data\": \"Hello Weird!\" }}", authToken, clientMsgId);

            response = await socketHandler.OnReceiveMessage(userId, connectionId, message);

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);

            await socketHandler.OnDisconnect(userId, connectionId);
            connSocket = await connMgr.GetSocketByIdAsync(userId, connectionId);

            Assert.Null(connSocket);




        }
    }
}
