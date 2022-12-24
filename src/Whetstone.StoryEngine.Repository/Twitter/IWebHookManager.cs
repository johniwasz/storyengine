using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Twitter;

namespace Whetstone.StoryEngine.Repository.Twitter
{
    public interface IWebHookManager
    {

        Task<TwitterWebhookListItem> RegisterWebhookAsync(TwitterCredentialListItem creds, string environment, Uri webhookUri);

        Task<IEnumerable<TwitterWebhookListItem>> GetWebHooksAsync(TwitterCredentialListItem creds, string environment);

        Task DeleteWebhookAsync(TwitterCredentialListItem creds, string environment, string webhookId);

        Task<GetSubscriptionsResponse> GetSubscriptionsAsync(TwitterCredentialListItem creds, string environment);


        Task SubscribeAsync(TwitterCredentialListItem creds, string environment);

        Task UnsubscribeAsync(TwitterCredentialListItem creds, string environment, long userId);

        Task ResendWebhookValidationAsync(TwitterCredentialListItem creds, string environment, string webhookId);
    }
}
