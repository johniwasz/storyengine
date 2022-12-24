using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Twitter;
using Whetstone.StoryEngine.Repository.Twitter;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class TwitterValidator : ITwitterValidator
    {
        private readonly ILogger<TwitterValidator> _logger;


        private readonly IWebHookManager _webHookManager;
        private readonly IOrganizationRepository _orgRepo;

        public TwitterValidator(IOrganizationRepository orgRepo, IWebHookManager webhookManager, ILogger<TwitterValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _webHookManager = webhookManager ?? throw new ArgumentNullException(nameof(webhookManager));

            _orgRepo = orgRepo ?? throw new ArgumentNullException(nameof(orgRepo));
        }

        public async Task<TwitterCrcResponse> GenerateTwitterCrcResponseAsync(Guid organizationId, Guid credentialId, string twitterCrc)
        {
            if (credentialId == default(Guid))
            {
                throw new ArgumentNullException(nameof(credentialId));
            }

            if (organizationId == default(Guid))
            {
                throw new ArgumentNullException(nameof(organizationId));
            }

            if (string.IsNullOrWhiteSpace(twitterCrc))
            {
                throw new ArgumentNullException(nameof(twitterCrc));
            }


            TwitterCrcResponse resp = null;


            try
            {


                //var retCreds = await _orgRepo.GetTwitterCredentialAsync(organizationId, credentialId, true);

                //if (retCreds != null)
                //{

                //    resp = GenerateCrcResponse(twitterCrc, retCreds.ConsumerSecret);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying twitter crc response");
                throw;
            }


            return resp;
        }


        /// <summary>
        /// This is used to create the response that verifies that the Twitter webhook is owned by the same organization that 
        /// registered the web hook.
        /// </summary>
        /// <param name="crcToken"></param>
        /// <param name="consumerSecret"></param>
        /// <returns></returns>
        private TwitterCrcResponse GenerateCrcResponse(string crcToken, string consumerSecret)
        {

            TwitterCrcResponse resp = null;
            var encoding = new ASCIIEncoding();

            byte[] keyBytes = encoding.GetBytes(consumerSecret);

            byte[] messageBytes = encoding.GetBytes(crcToken);

            string crcResponseToken = null;

            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                crcResponseToken = Convert.ToBase64String(hashmessage);
            }

            resp = new TwitterCrcResponse
            {
                ResponseToken = $"sha256={crcResponseToken}"
            };

            return resp;
        }
    }
}
