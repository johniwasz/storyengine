using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Data
{
    public interface IUserDataContext : IDisposable, IAsyncDisposable
    {
        DbSet<DataTitleClientUser> ClientUsers { get; set; }
        DbSet<OutboundBatchRecord> OutboundMessageBatch { get; set; }
        DbSet<OutboundMessageLogEntry> OutboundMessageLogs { get; set; }
        DbSet<OutboundMessagePayload> OutboundMessages { get; set; }
        DbSet<DataPhone> PhoneNumbers { get; set; }
        DbSet<EngineRequestAudit> SessionActions { get; set; }
        DbSet<EngineSession> Sessions { get; set; }
        DbSet<DataTitle> Titles { get; set; }
        DbSet<DataTitleVersionDeployment> TitleVersionDeployments { get; set; }
        DbSet<DataTitleVersion> TitleVersions { get; set; }
        DbSet<UserPhoneConsent> UserPhoneConsent { get; set; }

        DbSet<DataOrganization> Organizations { get; set; }

        DbSet<DataTwitterApplication> TwitterApplications { get; set; }

        DbSet<DataTwitterSubscription> TwitterSubscriptions { get; set; }

        DbSet<DataTwitterCredentials> TwitterCredentials { get; set; }

        DbSet<DataUser> Users { get; set; }

        DbSet<DataGroup> Groups { get; set; }

        DbSet<DataRole> Roles { get; set; }

        DbSet<DataFunctionalEntitlement> FunctionalEntitlements { get; set; }

        DbSet<DataFunctionalEntitlementRoleXRef> FunctionalEntitlementRoleXRefs { get; set; }

        DbSet<DataGroupRoleXRef> GroupRoleXRefs { get; set; }

        DbSet<DataSubscriptionLevel> SubscriptionLevels { get; set; }

        DbSet<DataTitleGroupXRef> TitleGroupXRefs { get; set; }

        DbSet<DataUserGroupXRef> UserGroupXRefs { get; set; }

        Task AddOutboundMessageLog(OutboundMessageLogEntry messageLogEntry);
        Task<List<MessageConsentReportRecord>> GetMessageConsentReportAsync(MessageConsentReportRequest reportRequest);
        Task LogEngineRequestAsync(Guid? engineUserId, Guid engineSessionId, Guid engineRequestId, string sessionId, string requestId, Guid deploymentId, string userId, string locale, string intentName, Dictionary<string, string> slots, long processDuration, DateTime? startTime, DateTime? actionTime, StoryRequestType? requestType, string preNodeActionLog, string mappedNode, string postNodeActionLog, YesNoMaybeEnum? canFulfill, Dictionary<string, SlotCanFulFill> slotFulfillment, float? intentConfidence, string rawText, Dictionary<string, string> requestAttributes, Dictionary<string, string> sessionAttributes, bool? isFirstSession, string requestBodyText, string responseBodyText, string engineErrorText, string responseConversionError, bool? isGuest);
        Task LogTwilioSmsCallbackAsync(string twilioMessageId, string queueMessageId, bool isException, int? httpStatus, MessageSendStatus messageStatus, string extendedStatus);
        Task UpsertPhoneConsentAsync(UserPhoneConsent phoneConsent);
        Task UpsertPhoneInfoAsync(DataPhone phoneInfo);
        Task UpsertTitleUsertAsync(DataTitleClientUser dataUser);

        Task SaveChangesAsync();

        Task CreateUserAccountAsync(DataUser user, string accountName);


        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;


        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;


        Task<ProjectVersionFileMapping> GetProjectVersionMapping(Guid projectId, Guid versionId);

    }
}