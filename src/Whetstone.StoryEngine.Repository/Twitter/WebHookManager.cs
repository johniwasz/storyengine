using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Twitter;

namespace Whetstone.StoryEngine.Repository.Twitter
{
    public class WebHookManager : IWebHookManager
    {
        public async Task<TwitterWebhookListItem> RegisterWebhookAsync(TwitterCredentialListItem creds, string environment, Uri webhookUri)
        {
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));

            if (webhookUri == null)
                throw new ArgumentNullException(nameof(webhookUri));

            creds.BearerToken = null;
            var userClient = GetTwitterClient(creds);

            IWebhook registeredHook = await
                        userClient.AccountActivity.CreateAccountActivityWebhookAsync(environment, webhookUri.AbsoluteUri);

            TwitterWebhookListItem hookItem = new TwitterWebhookListItem()
            {

                Id = registeredHook.Id,
                Url = registeredHook.Url,
                Valid = registeredHook.Valid,
                CreatedAt = registeredHook.CreatedAt
            };

            return hookItem;
        }

        public async Task<IEnumerable<TwitterWebhookListItem>> GetWebHooksAsync(TwitterCredentialListItem creds, string environment)
        {
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));



            var userClient = GetTwitterClient(creds);

            List<TwitterWebhookListItem> webHooks = new List<TwitterWebhookListItem>();

            List<IWebhook> hooks = (await userClient.AccountActivity.GetAccountActivityEnvironmentWebhooksAsync(environment)).ToList();

            foreach (var hook in hooks)
            {
                TwitterWebhookListItem hookItem = new TwitterWebhookListItem()
                {

                    Id = hook.Id,
                    Url = hook.Url,
                    Valid = hook.Valid,
                    CreatedAt = hook.CreatedAt
                };

                webHooks.Add(hookItem);
            }



            return webHooks;
        }

        public async Task DeleteWebhookAsync(TwitterCredentialListItem creds, string environment, string webhookId)
        {
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            var userClient = GetTwitterClient(creds.BearerToken);
            await userClient.AccountActivity.DeleteAccountActivityWebhookAsync(environment, webhookId);
        }


        public async Task ResendWebhookValidationAsync(TwitterCredentialListItem creds, string environment, string webhookId)
        {
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            var userClient = GetTwitterClient(creds);

            await userClient.AccountActivity.TriggerAccountActivityWebhookCRCAsync(environment, webhookId);
        }

        public async Task<GetSubscriptionsResponse> GetSubscriptionsAsync(TwitterCredentialListItem creds, string environment)
        {
            GetSubscriptionsResponse retSubs = new GetSubscriptionsResponse();

            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));

            var userClient = GetTwitterClient(creds);



            IWebhookEnvironmentSubscriptions subscriptions = await userClient.AccountActivity.GetAccountActivitySubscriptionsAsync(environment);

            retSubs.ApplicationId = subscriptions.ApplicationId;
            retSubs.Subscriptions = new List<SubscriptionResponse>();
            foreach (var sub in subscriptions.Subscriptions)
            {
                SubscriptionResponse subResp = new SubscriptionResponse();
                subResp.UserId = sub.UserId;

                retSubs.Subscriptions.Add(subResp);

            }

            return retSubs;
        }


        public async Task SubscribeAsync(TwitterCredentialListItem creds, string environment)
        {

            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));

            var userClient = GetTwitterClient(creds);

            await userClient.AccountActivity.SubscribeToAccountActivityAsync(environment);
        }

        public async Task UnsubscribeAsync(TwitterCredentialListItem creds, string environment, long userId)
        {

            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));

            var userClient = GetTwitterClient(creds);

            await userClient.AccountActivity.UnsubscribeFromAccountActivityAsync(environment, userId);
        }

        private TwitterClient GetTwitterClient(TwitterCredentialListItem creds)
        {

            Tweetinvi.Models.TwitterCredentials inviCreds = new Tweetinvi.Models.TwitterCredentials(creds.ConsumerKey, creds.ConsumerSecret, creds.AccessToken, creds.AccessTokenSecret)
            {
                BearerToken = creds.BearerToken
            };

            var userClient = new TwitterClient(inviCreds);


            return userClient;

        }

        private TwitterClient GetTwitterClient(string bearerToken)
        {

            Tweetinvi.Models.TwitterCredentials inviCreds = new Tweetinvi.Models.TwitterCredentials()
            {
                BearerToken = bearerToken
            };

            var userClient = new TwitterClient(inviCreds);


            return userClient;

        }



    }
}
