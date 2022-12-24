using Amazon;
using Amazon.Auth.AccessControlPolicy;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statement = Amazon.Auth.AccessControlPolicy.Statement;

namespace Whetstone.StoryEngine.ConfigUtilities.KeyPolicy
{
    public static class KeyPolicyManager
    {

        private const string EncryptSid = "Allows encrypt";
        private const string DecryptSid = "Allows decrypt";

        private const string REQUIREDPREFIX = "arn:aws:iam:";

        private static readonly List<string> EncryptRights = new List<string> { "kms:GenerateDataKey", "kms:Encrypt", "kms:Reencrypt" };

        private static readonly List<string> DecryptRights = new List<string> { "kms:Decrypt" };




        #region Remove grant management
        public static async Task DeleteKeyManagerAsync(KeyPolicyRequest policyRequest, ILambdaLogger logger)
        {
            if (policyRequest == null)
                throw new ArgumentException($"{nameof(policyRequest)} cannot be null");

            if (string.IsNullOrWhiteSpace(policyRequest.Key))
                throw new ArgumentException($"{nameof(policyRequest)} Key property cannot be null or empty");

            if (string.IsNullOrWhiteSpace(policyRequest.RoleArn))
                throw new ArgumentException($"{nameof(policyRequest)} RoleArn property cannot be null or empty");


            var policyResp = await GetDefaultPolicyAsync(policyRequest.Key, logger);



            var keyPolicy = policyResp.Item1;

            SanitizePolicy(keyPolicy);



            string policyName = policyResp.Item2;

            switch (policyRequest.GrantType)
            {
                case KeyGrantType.Decrypt:
                    RemoveRoleArn(EncryptSid, keyPolicy, policyRequest.RoleArn, logger);
                    break;
                case KeyGrantType.Encrypt:
                    RemoveRoleArn(DecryptSid, keyPolicy, policyRequest.RoleArn, logger);
                    break;
                case KeyGrantType.EncryptDecrypt:
                    RemoveRoleArn(EncryptSid, keyPolicy, policyRequest.RoleArn, logger);
                    RemoveRoleArn(DecryptSid, keyPolicy, policyRequest.RoleArn, logger);
                    break;

            }

            await UpdatePolicyAsync(policyRequest.Key, keyPolicy, policyName);
        }

        /// <summary>
        /// Remove any invalid principals.
        /// </summary>
        /// <param name="keyPolicy"></param>
        private static void SanitizePolicy(Policy keyPolicy)
        {
            if (keyPolicy.Statements != null)
            {
                // If a statement is missing principals, then flag it for removal.

                List<Statement> remStatements = new List<Statement>();
                foreach (var statement in keyPolicy.Statements)
                {

                    var prinsToRemove = statement.Principals.Where(x => !x.Id.StartsWith(REQUIREDPREFIX)).ToList();

                    foreach (var badPrin in prinsToRemove)
                    {
                        statement.Principals.Remove(badPrin);
                    }




                    if (statement.Principals.Count == 0)
                        remStatements.Add(statement);

                }

                foreach (var remStatement in remStatements)
                {
                    keyPolicy.Statements.Remove(remStatement);
                }

            }


        }

        private static void RemoveRoleArn(string statementSid, Policy keyPolicy, string roleArn, ILambdaLogger logger)
        {
            var foundStatement = keyPolicy?.Statements?.FirstOrDefault(x => x.Id.Equals(statementSid));

            var foundPrin =
                foundStatement?.Principals?.FirstOrDefault(x =>
                    x.Id.Equals(roleArn, StringComparison.OrdinalIgnoreCase));


            // Check the other principals in the list. If they are not well-formatted, then remove them.
            if (foundStatement?.Principals != null)
            {
                var prinsToRemove = foundStatement.Principals.Where(x => !x.Id.StartsWith(REQUIREDPREFIX)).ToList();

                foreach (var removePrin in prinsToRemove)
                    foundStatement.Principals.Remove(removePrin);
            }



            if (foundPrin != null)
            {
                logger.Log($"Removing role {roleArn} from grant {statementSid}");
                foundStatement.Principals.Remove(foundPrin);

                // If the last principal is being removed from the statement, then remove the statement.
                // Statements must contain a minimum of one principal.
                if (foundStatement.Principals.Count == 0)
                {
                    logger.Log($"Last role removed from grant {statementSid}. Removing grant from policy.");
                    keyPolicy.Statements.Remove(foundStatement);
                }

            }
        }
        #endregion

        public static async Task UpdateKeyManagerAsync(KeyPolicyRequest policyRequest, ILambdaLogger logger)
        {
            if (policyRequest == null)
                throw new ArgumentException($"{nameof(policyRequest)} cannot be null");

            if (string.IsNullOrWhiteSpace(policyRequest.Key))
                throw new ArgumentException($"{nameof(policyRequest)} Key property cannot be null or empty");

            if (string.IsNullOrWhiteSpace(policyRequest.RoleArn))
                throw new ArgumentException($"{nameof(policyRequest)} RoleArn property cannot be null or empty");


            // Make sure the ARN passed to the policyRequest is a role that exists
            bool isValidRole = await IsValidRoleAsync(policyRequest.RoleArn, logger);

            if (!isValidRole)
                throw new InvalidRoleException($"Role {policyRequest.RoleArn} could not be verified", policyRequest.RoleArn);

            var policyResp = await GetDefaultPolicyAsync(policyRequest.Key, logger);

            var keyPolicy = policyResp.Item1;

            string policyName = policyResp.Item2;

            switch (policyRequest.GrantType)
            {
                case KeyGrantType.Decrypt:
                    GrantRights(keyPolicy, DecryptSid, policyRequest.RoleArn, DecryptRights, logger);
                    break;
                case KeyGrantType.Encrypt:
                    GrantRights(keyPolicy, EncryptSid, policyRequest.RoleArn, EncryptRights, logger);
                    break;
                case KeyGrantType.EncryptDecrypt:
                    GrantRights(keyPolicy, DecryptSid, policyRequest.RoleArn, DecryptRights, logger);
                    GrantRights(keyPolicy, EncryptSid, policyRequest.RoleArn, EncryptRights, logger);
                    break;
            }

            await UpdatePolicyAsync(policyRequest.Key, keyPolicy, policyName);
        }


        private static async Task<bool> IsValidRoleAsync(string roleArn, ILambdaLogger logger)
        {
            bool isValid = false;
            RegionEndpoint region = StoryEngine.ContainerSettingsReader.GetAwsDefaultEndpoint();

            using (var iamClient = new AmazonIdentityManagementServiceClient(region))
            {
                // Get the last portion of the role Arn
                string[] parsedRole = roleArn.Split('/');

                string lastPortion = parsedRole[parsedRole.Length - 1];

                GetRoleRequest roleReq = new GetRoleRequest
                {
                    RoleName = lastPortion
                };

                GetRoleResponse roleResp = null;

                try
                {
                    roleResp = await iamClient.GetRoleAsync(roleReq);
                }
                catch (NoSuchEntityException)
                {
                    logger.Log($"Role {roleArn} not found");
                }

                if (roleResp?.Role != null)
                {
                    isValid = true;
                    logger.Log($"Role {roleArn} is verified.");
                }
            }

            return isValid;
        }

        private static void GrantRights(Policy keyPolicy, string sid, string roleArn, List<string> rights,
            ILambdaLogger logger)
        {
            Statement grantStatement = keyPolicy.Statements?.FirstOrDefault(x => x.Id.Equals(sid));

            List<Principal> existingPrins = new List<Principal>();

            if (grantStatement != null)
            {
                existingPrins.AddRange(grantStatement.Principals);
                keyPolicy.Statements.Remove(grantStatement);
            }

            logger.Log($"Creating new grant {sid} for role {roleArn}");
            grantStatement = new Statement(Statement.StatementEffect.Allow)
            {
                Id = sid,
                Actions = new List<ActionIdentifier>()
            };


            if (keyPolicy.Statements == null)
                keyPolicy.Statements = new List<Statement>();

            grantStatement.Resources = new List<Resource>
            {
                new Resource("*")
            };


            if (grantStatement.Principals == null)
                grantStatement.Principals = new List<Principal>();

            foreach (var prin in existingPrins)
            {
                //If one of the principals is not well-formatted, then do not add it.
                //Only add it if it is well formatted.
                if (prin.Id.StartsWith(REQUIREDPREFIX))
                    grantStatement.Principals.Add(prin);
            }

            // Reapply actions
            if (grantStatement.Actions == null)
                grantStatement.Actions = new List<ActionIdentifier>();

            grantStatement.Actions.Clear();

            foreach (string right in rights)
                grantStatement.Actions.Add(new ActionIdentifier(right));


            // If the principal does not exist, then add it.
            var foundPrin =
                grantStatement.Principals.FirstOrDefault(x => x.Id.Equals(roleArn) && x.Provider.Equals("AWS"));


            if (foundPrin == null)
            {
                grantStatement.Principals.Add(new Principal("AWS", roleArn, false));
                logger.Log($"Adding role {roleArn} to grant {sid}");
            }
            else
                logger.Log($"Adding role {roleArn} is already granted {sid}");


            keyPolicy.Statements.Add(grantStatement);



        }

        private static async Task UpdatePolicyAsync(string keyArn, Policy keyPolicy, string policyName)
        {

            RegionEndpoint region = StoryEngine.ContainerSettingsReader.GetAwsDefaultEndpoint();

            PutKeyPolicyRequest updatePolicy = new PutKeyPolicyRequest
            {
                Policy = keyPolicy.ToJson(true),
                KeyId = keyArn,
                PolicyName = policyName
            };

            using (var keyClient = new AmazonKeyManagementServiceClient(region))
            {
                var putPolicyResp = await keyClient.PutKeyPolicyAsync(updatePolicy);
            }

        }

        private static async Task<(Policy, string)> GetDefaultPolicyAsync(string keyArn, ILambdaLogger logger)
        {
            RegionEndpoint region = StoryEngine.ContainerSettingsReader.GetAwsDefaultEndpoint();

            string policyJson = null;
            string policyName = null;
            ListKeyPoliciesRequest listPoliciesReq = new ListKeyPoliciesRequest
            {
                KeyId = keyArn
            };

            using (var keyClient = new AmazonKeyManagementServiceClient(region))
            {

                ListKeyPoliciesResponse policiesResp = await keyClient.ListKeyPoliciesAsync(listPoliciesReq);

                // Edit the first policy. At the time of this writing (8/30/2019) keys can only have
                // one policy associated with it.
                policyName = policiesResp.PolicyNames[0];

                GetKeyPolicyRequest getRequest = new GetKeyPolicyRequest
                {
                    KeyId = keyArn,
                    PolicyName = policyName
                };

                var policyResp = await keyClient.GetKeyPolicyAsync(getRequest);

                policyJson = policyResp.Policy;


            }


            var keyPolicy = Policy.FromJson(policyJson);

            logger.Log($"Found policy {policyName} with id {keyPolicy.Id} for key {keyArn}");

            return (keyPolicy, policyName);
        }
    }
}
