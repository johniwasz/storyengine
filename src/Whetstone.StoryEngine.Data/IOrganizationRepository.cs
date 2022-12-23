using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Twitter;

namespace Whetstone.StoryEngine.Data
{
    public interface IOrganizationRepository
    {
        Task<IEnumerable<Organization>> GetOrganizationsAsync();

   //     Task<IEnumerable<TwitterCredentialListItem>> GetTwitterCredentialsAsync(Guid organizationId);

    //    Task<TwitterCredentialListItem> GetTwitterCredentialAsync(Guid organizationId, Guid credentialId, bool? bypassSecurity = false);

     //   Task DeleteTwitterCredentialsAsync(Guid organizationId, Guid credentialsId);


   //     Task<AddTwitterCredentialsResponse> AddTwitterCredentialsAsync(Guid organizationId, AddTwitterCredentialsRequest addCredsRequest);

   //     Task<IEnumerable<TwitterWebhookListItem>> GetTwitterWebhooksAsync(Guid organizationId, Guid credentialsId, string environment);

  //      Task DeleteTwitterWebhookAsync(Guid organizationId, Guid credentialsId, string environment, string webhookId);

   //     Task<TwitterWebhookListItem> RegisterTwitterWebhookAsync(Guid organizationId, Guid credentialId, string environment, Uri url);

    //    Task<GetSubscriptionsResponse> GetTwitterSubscriptionsAsync(Guid organizationId, Guid credentialId, string environment);

  //      Task SubscribeAsync(Guid organizationId, Guid credentialsId, string environment);

    //    Task UnsubscribeAsync(Guid organizationId, Guid credentialsId, string environment, long userId);

      //  Task ResendWebhookValidationAsync(Guid organizationId, Guid credentialsId, string environment, string webhookId);

    }
}
