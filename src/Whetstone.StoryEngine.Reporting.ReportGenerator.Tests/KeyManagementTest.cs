using Amazon;
using Amazon.Auth.AccessControlPolicy;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator.Tests
{
    public class KeyManagementTest
    {

        [Fact]
        public async Task GetKeyPolicyTest()
        {

            DescribeKeyRequest dsRequest = new DescribeKeyRequest();
            dsRequest.KeyId = "alias/devEnvironmentKey";
            string keyArn = null;
            using (var keyClient = new AmazonKeyManagementServiceClient(RegionEndpoint.USEast1))
            {
                var keyResp = await keyClient.DescribeKeyAsync(dsRequest);
                keyArn = keyResp.KeyMetadata.Arn;
            }


            ListKeyPoliciesRequest listPoliciesReq = new ListKeyPoliciesRequest();
            listPoliciesReq.KeyId = keyArn;

            using (var keyClient = new AmazonKeyManagementServiceClient(RegionEndpoint.USEast1))
            {

                ListKeyPoliciesResponse policiesResp = await keyClient.ListKeyPoliciesAsync(listPoliciesReq);

                foreach (string policyName in policiesResp.PolicyNames)
                {
                    Debug.WriteLine(policyName);


                    GetKeyPolicyRequest policyRequest = new GetKeyPolicyRequest
                    {
                        KeyId = keyArn,
                        PolicyName = policyName
                    };

                    var policyResp = await keyClient.GetKeyPolicyAsync(policyRequest);


                    string policyDoc = policyResp.Policy;

                    var keyPolicy = Policy.FromJson(policyDoc);


                    // Add a new statement....
                    Statement policyStatement = new Statement(Statement.StatementEffect.Allow);
                    policyStatement.Resources = new List<Resource>();
                    policyStatement.Resources.Add(new Resource("*"));
                    policyStatement.Id = "newstatement";

                    policyStatement.Actions = new List<ActionIdentifier>();
                    policyStatement.Actions.Add(new ActionIdentifier($"kms:{GrantOperation.Decrypt.Value}"));
                    policyStatement.Actions.Add(new ActionIdentifier($"kms:{GrantOperation.Encrypt.Value}"));
                    policyStatement.Actions.Add(new ActionIdentifier($"kms:{GrantOperation.GenerateDataKey.Value}"));


                    policyStatement.Principals.Add(new Principal("AWS", "arn:aws:iam::940085449815:role/dummyrole"));

                    keyPolicy.Statements.Add(policyStatement);


                    PutKeyPolicyRequest updatePolicy = new PutKeyPolicyRequest();
                    updatePolicy.Policy = keyPolicy.ToJson(true);
                    updatePolicy.KeyId = keyArn;
                    updatePolicy.PolicyName = "newpolicy";



                    var putPolicyResp = await keyClient.PutKeyPolicyAsync(updatePolicy);
                }

            }


        }
    }
}
