using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.Test.Config
{

    public class DatabaseConReaderTest
    {
        [Fact]
        public async Task ReadDbConnectionStoreAsync()
        {

            string envString = "dev";

            // The parameter name is customized based on the ASPNETCORE_ENVIRONMENT
            //
            // You can change this to a fixed string or use a different mechanism
            // to customize.
            String parameterName = $"/storyengine/{envString}/enginedb";

            // Using USEast1
            var ssmClient = new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.USEast1);

            var response = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = true
            });

            string paramVal = response.Parameter.Value;
            

        }

    }
}
