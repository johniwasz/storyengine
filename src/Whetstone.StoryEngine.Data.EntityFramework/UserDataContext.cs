using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.EntityFramework.MigrationOperations;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class UserDataContext : DbContext, IUserDataContext
    {
        /// <summary>
        /// This is the error code raise by PostgeSQL when a duplicate key error is found.
        /// </summary>
        public static readonly string POSTGESQL_CODE_DUPLICATEKEY = "23505";

        private static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole();
        }
        );


        private readonly ILogger<UserDataContext> _logger;

        public DbSet<DataTitleClientUser> ClientUsers { get; set; }

        public DbSet<DataPhone> PhoneNumbers { get; set; }

        public DbSet<UserPhoneConsent> UserPhoneConsent { get; set; }

        public DbSet<EngineSession> Sessions { get; set; }

        public DbSet<EngineRequestAudit> SessionActions { get; set; }

        public DbSet<OutboundBatchRecord> OutboundMessageBatch { get; set; }

        public DbSet<OutboundMessagePayload> OutboundMessages { get; set; }

        public DbSet<OutboundMessageLogEntry> OutboundMessageLogs { get; set; }

        public DbSet<DataTitle> Titles { get; set; }

        public DbSet<DataUser> Users { get; set; }

        public DbSet<DataOrganization> Organizations { get; set; }

        public DbSet<DataTwitterCredentials> TwitterCredentials { get; set; }

        public DbSet<DataTwitterApplication> TwitterApplications { get; set; }

        public DbSet<DataTwitterSubscription> TwitterSubscriptions { get; set; }

        public DbSet<DataGroup> Groups { get; set; }

        public DbSet<DataRole> Roles { get; set; }

        public DbSet<DataFunctionalEntitlement> FunctionalEntitlements { get; set; }

        public DbSet<DataFunctionalEntitlementRoleXRef> FunctionalEntitlementRoleXRefs { get; set; }

        public DbSet<DataGroupRoleXRef> GroupRoleXRefs { get; set; }

        public DbSet<DataSubscriptionLevel> SubscriptionLevels { get; set; }

        public DbSet<DataTitleGroupXRef> TitleGroupXRefs { get; set; }

        public DbSet<DataUserGroupXRef> UserGroupXRefs { get; set; }

        public DbSet<DataTitleVersion> TitleVersions { get; set; }

        public DbSet<DataTitleVersionDeployment> TitleVersionDeployments { get; set; }

        private DbContextOptions<UserDataContext> _options;


        public UserDataContext(DbContextOptions<UserDataContext> options, ILogger<UserDataContext> logger) : base(options)
        {
            _options = options;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {

            options
                .ReplaceService<IMigrationsSqlGenerator, PostGresEngineMigrationGenerator>();
#if DEBUG
            options.UseLoggerFactory(loggerFactory) //tie-up DbContext with LoggerFactory object
                .EnableSensitiveDataLogging();
#endif

            base.OnConfiguring(options);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.HasDefaultSchema("whetstone");

            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.HasPostgresExtension("hstore");

            //  modelBuilder.Entity<DataUser>().HasAlternateKey(uix => new { uix.UserId, uix.TitleId }).HasName("APK_UserTitle");

            modelBuilder.Entity<DataTitleClientUser>()
                .Property(typeof(string), "TitleStateJson")
                .HasColumnType("jsonb")
                .HasColumnName("titlecrumbs");

            modelBuilder.Entity<DataFunctionalEntitlementRoleXRef>()
                .HasKey(idx =>
                    new { idx.RoleId, idx.FunctionalEntitlementId }).HasName("pk_role_funcentitlement");

            modelBuilder.Entity<DataFunctionalEntitlementRoleXRef>()
                .HasOne<DataFunctionalEntitlement>(x => x.FunctionalEntitlement)
                .WithMany(u => u.FunctionalEntitlementRoleXRefs)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.FunctionalEntitlementId);

            modelBuilder.Entity<DataFunctionalEntitlementRoleXRef>()
                .HasOne<DataRole>(x => x.Role)
                .WithMany(u => u.FunctionalEntitlementRoleXRefs)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.RoleId);


            modelBuilder.Entity<DataUserGroupXRef>()
                .HasKey(key => new { key.GroupId, key.UserId }).HasName("pk_user_group");


            modelBuilder.Entity<DataUserGroupXRef>()
                .HasOne<DataUser>(x => x.User)
                .WithMany(u => u.GroupXRefs)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.UserId);


            modelBuilder.Entity<DataUserGroupXRef>()
                .HasOne<DataGroup>(x => x.Group)
                .WithMany(u => u.UserGroupXRefs)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.GroupId);

            modelBuilder.Entity<DataTitleGroupXRef>()
                .HasKey(idx =>
                    new { idx.GroupId, idx.TitleId }).HasName("pk_group_title");


            modelBuilder.Entity<DataTitleGroupXRef>()
                .HasOne<DataTitle>(x => x.Title)
                .WithMany(u => u.TitleGroupXRef)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.TitleId);


            modelBuilder.Entity<DataTitleGroupXRef>()
                .HasOne<DataGroup>(x => x.Group)
                .WithMany(u => u.TitleGroupXRef)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.GroupId);


            modelBuilder.Entity<DataGroupRoleXRef>()
                .HasKey(idx =>
                    new { idx.GroupId, idx.RoleId }).HasName("pk_group_role");

            modelBuilder.Entity<DataGroupRoleXRef>()
                .HasOne<DataRole>(x => x.Role)
                .WithMany(u => u.GroupRoleXRef)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.RoleId);


            modelBuilder.Entity<DataGroupRoleXRef>()
                .HasOne<DataGroup>(x => x.Group)
                .WithMany(u => u.GroupRoleXRefs)
                .OnDelete(DeleteBehavior.ClientCascade)
                .HasForeignKey(u => u.GroupId);

            modelBuilder.Entity<DataTitleClientUser>()
                .Property(typeof(string), "PermanentTitleStateJson")
                .HasColumnType("jsonb")
                .HasColumnName("permanenttitlecrumbs");

            modelBuilder.Entity<DataTitleClientUser>()
                .Property(x => x.UserId)
                .IsRequired(true);


            modelBuilder.Entity<DataTitleClientUser>()
                .Property(x => x.TitleId)
                .IsRequired(true);

            modelBuilder.Entity<DataTitleClientUser>()
                .Property(x => x.Client)
                .IsRequired(true);


            modelBuilder.Entity<DataTitleClientUser>()
                .HasIndex(idx =>
                new { idx.TitleId, idx.Client, idx.UserId })
                .IsUnique(true);

            modelBuilder.Entity<OutboundMessageLogEntry>()
                .HasIndex(x => x.LogTime)
                .IsUnique(false);


            modelBuilder.Entity<DataPhone>()
                .HasIndex(idx => idx.PhoneNumber)
                .IsUnique(true);

            modelBuilder.Entity<EngineRequestAudit>()
                .HasKey(ikey => new { ikey.SessionId, ikey.RequestId });

            modelBuilder.Entity<EngineRequestAudit>()
                .HasIndex(idx => idx.SessionId)
                .IsUnique(false);

            modelBuilder.Entity<DataTitle>()
                .HasIndex(idx => idx.ShortName)
                .IsUnique(true);

            modelBuilder.Entity<DataTitleVersion>()
                .HasIndex(idx => new { idx.TitleId, idx.Version })
                .IsUnique(true);


            modelBuilder.Entity<EngineRequestAudit>()
                .Property(x => x.SessionId)
                .IsRequired();

            modelBuilder.Entity<EngineRequestAudit>()
                .Property(typeof(string), "SlotFulFillmentJson")
                .HasColumnType("jsonb")
                .HasColumnName("slotfulfillment");

            modelBuilder.Entity<UserPhoneConsent>()
                .HasOne<DataTitleClientUser>(sc => sc.TitleUser)
                .WithMany(s => s.TitleUserPhones)
                .HasForeignKey(sc => sc.TitleClientUserId);


            modelBuilder.Entity<UserPhoneConsent>()
                .HasOne<DataPhone>(sc => sc.Phone)
                .WithMany(s => s.ConsentRecords)
                .HasForeignKey(sc => sc.PhoneId);



            modelBuilder.Entity<OutboundBatchRecord>()
                .HasOne(e => e.Consent)
                .WithMany(c => c.OutboundMessageBatches)
                .HasForeignKey(fk => fk.ConsentId);


            modelBuilder.Entity<OutboundBatchRecord>()
                .HasOne(e => e.SentFromPhone)
                .WithMany(c => c.SentFromSmsBatches)
                .HasForeignKey(fk => fk.SmsFromNumberId);

            modelBuilder.Entity<OutboundBatchRecord>()
                .HasOne(e => e.SentToPhone)
                .WithMany(c => c.SentToSmsBatches)
                .HasForeignKey(fk => fk.SmsToNumberId);

            modelBuilder.Entity<EngineRequestAudit>()
                .Property(x => x.Id)
                .IsRequired(true);




        }



        public async Task<ProjectVersionFileMapping> GetProjectVersionMapping(Guid projectId, Guid versionId)
        {

            if (projectId == default(Guid))
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");

            if (versionId == default(Guid))
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");

            ProjectVersionFileMapping dataVersion = null;


            dataVersion = await this.TitleVersions.Join(this.Titles,
                tv => tv.TitleId,
            t => t.Id,
            (tv, t) => new
            {
                Title = t,
                TitleVerion = tv
            })
            .Where(t => t.Title.Id == projectId
                && t.TitleVerion.Id == versionId)
                .Select(t => new ProjectVersionFileMapping { Version = t.TitleVerion.Version, ProjectAlias = t.Title.ShortName })
                .SingleOrDefaultAsync();



            return dataVersion;


        }


        public async Task LogTwilioSmsCallbackAsync(string twilioMessageId, string queueMessageId, bool isException, int? httpStatus, MessageSendStatus messageStatus, string extendedStatus)
        {
            if (string.IsNullOrWhiteSpace(twilioMessageId))
                throw new ArgumentNullException(nameof(twilioMessageId));

            if (string.IsNullOrWhiteSpace(queueMessageId))
                throw new ArgumentNullException(nameof(queueMessageId));

            using NpgsqlConnection pgcon = (NpgsqlConnection)this.Database.GetDbConnection();
            NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.addsmstwiliolog", pgcon);
            pgcom.CommandType = CommandType.StoredProcedure;

            //create or replace function whetstone.addsmstwiliolog(twiliomessageid text, isexception boolean, httpstatus int, extendedstatus text,
            //messagestatus int, queuemessageid text)

            pgcom.Parameters.AddWithValue("twiliomessageid", NpgsqlTypes.NpgsqlDbType.Text, twilioMessageId);
            pgcom.Parameters.AddWithValue("isexception", NpgsqlTypes.NpgsqlDbType.Boolean, isException);


            pgcom.Parameters.AddWithValue("messagestatus", NpgsqlTypes.NpgsqlDbType.Integer, (long)messageStatus);

            if (httpStatus.HasValue)
                pgcom.Parameters.AddWithValue("httpstatus", NpgsqlTypes.NpgsqlDbType.Integer, httpStatus);
            else
                pgcom.Parameters.AddWithValue("httpstatus", NpgsqlTypes.NpgsqlDbType.Integer, DBNull.Value);


            if (!string.IsNullOrWhiteSpace(extendedStatus))
                pgcom.Parameters.AddWithValue("extendedstatus", NpgsqlTypes.NpgsqlDbType.Text, extendedStatus);
            else
                pgcom.Parameters.AddWithValue("extendedstatus", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);


            pgcom.Parameters.AddWithValue("queuemessageid", NpgsqlTypes.NpgsqlDbType.Text, queueMessageId);



            Stopwatch dbTime = new Stopwatch();
            try
            {
                dbTime.Start();
                await pgcon.OpenAsync();
                await pgcom.ExecuteNonQueryAsync();
            }
            finally
            {
                dbTime.Stop();
                pgcon.Close();
            }




        }


        public async Task<List<MessageConsentReportRecord>> GetMessageConsentReportAsync(MessageConsentReportRequest reportRequest)
        {
            List<MessageConsentReportRecord> returnList = null;

            if (reportRequest == null)
                throw new ArgumentNullException(nameof(reportRequest));

            if (reportRequest.TitleId == default(Guid))
                throw new ArgumentNullException(nameof(reportRequest), "TitleId property must have a valid value");

            if (reportRequest.StartTime == default(DateTime))
                throw new ArgumentNullException(nameof(reportRequest), "StartTime property must have a valid value");

            if (reportRequest.EndTime == default(DateTime))
                throw new ArgumentNullException(nameof(reportRequest), "EndTime property must have a valid value");


            if (reportRequest.StartTime > reportRequest.EndTime)
                throw new ArgumentNullException(nameof(reportRequest), "StartTime property value must come before the EndTime property value");


            await using (NpgsqlConnection pgcon = (NpgsqlConnection)this.Database.GetDbConnection())
            {


                NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.getconsentreport", pgcon)
                {
                    CommandType = CommandType.StoredProcedure
                };

                //ptitleid
                pgcom.Parameters.AddWithValue("ptitleid", NpgsqlTypes.NpgsqlDbType.Uuid, reportRequest.TitleId);


                //pstarttime
                pgcom.Parameters.AddWithValue("pstarttime", NpgsqlTypes.NpgsqlDbType.Timestamp,
                    reportRequest.StartTime);

                //pendtime
                pgcom.Parameters.AddWithValue("pendtime", NpgsqlTypes.NpgsqlDbType.Timestamp,
                    reportRequest.EndTime);

                Stopwatch dbTime = new Stopwatch();
                DbDataReader reportReader = null;
                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();
                    using (reportReader = await pgcom.ExecuteReaderAsync())
                    {
                        //mc."status" as successstatus, mc.userid, mc.phonenumber, mc.sendtime, mc.providermessageid, 
                        // mc.code, mc.sessionid, mc.smsconsentdate
                        returnList = new List<MessageConsentReportRecord>();
                        if (reportReader.HasRows)
                        {


                            while (reportReader.Read())
                            {
                                bool repStatus = reportReader.GetBoolean(0);
                                Guid? userId = await reportReader.GetFieldValueAsync<Guid?>(1);
                                string phoneNumber = reportReader.GetString(2);
                                DateTime? sendTime = await reportReader.GetFieldValueAsync<DateTime?>(3);
                                string providerMessageId = reportReader.GetString(4);
                                string sentMessage = reportReader.GetString(5);
                                Guid? sessionId = await reportReader.GetFieldValueAsync<Guid?>(6);
                                DateTime? consentDate = await reportReader.GetFieldValueAsync<DateTime?>(7);
                                MessageConsentReportRecord reportRecord = new MessageConsentReportRecord
                                {
                                    Status = repStatus,
                                    UserId = userId,
                                    PhoneNumber = phoneNumber,
                                    SendTime = sendTime,
                                    ProviderMessageId = providerMessageId,
                                    Message = sentMessage,
                                    SessionId = sessionId,
                                    SmsConsentDate = consentDate
                                };

                                returnList.Add(reportRecord);

                            }

                        }

                    }


                }
                finally
                {
                    if (reportReader != null)
                    {
                        if (!reportReader.IsClosed)
                            reportReader.Close();
                    }

                    dbTime.Stop();
                    pgcon.Close();
                }

                dbTime.Stop();
                _logger.LogInformation($"GetConsentReport database time: {dbTime.ElapsedMilliseconds}");

            }


            return returnList;
        }


        public async Task UpsertTitleUsertAsync(
            DataTitleClientUser dataUser)
        {

            if (dataUser == null)
                throw new ArgumentNullException(nameof(dataUser));

            if (!dataUser.Id.HasValue)
                throw new ArgumentNullException(nameof(dataUser), "Id property must have a value");

            if (string.IsNullOrWhiteSpace(dataUser.UserId))
                throw new ArgumentNullException(nameof(dataUser), "UserId property must not be null or empty");

            if (dataUser.TitleId == default(Guid))
                throw new ArgumentNullException(nameof(dataUser), "TitleId property must have a valid value");

            if (dataUser.LastAccessedDate == default(DateTime))
                throw new ArgumentNullException(nameof(dataUser), "LastAccessedDate property must have a valid value");

            if (dataUser.CreatedTime == default(DateTime))
                throw new ArgumentNullException(nameof(dataUser), "CreatedTime property must have a valid value");




            using (NpgsqlConnection pgcon = (NpgsqlConnection)this.Database.GetDbConnection())
            {


                NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.addupdatetitleuser", pgcon)
                {
                    CommandType = CommandType.StoredProcedure
                };


                // p_id
                pgcom.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Uuid, dataUser.Id.Value);

                // p_hashkey text
                if (!string.IsNullOrWhiteSpace(dataUser.HashKey))
                    pgcom.Parameters.AddWithValue("p_hashkey", NpgsqlTypes.NpgsqlDbType.Text, dataUser.HashKey);
                else
                    pgcom.Parameters.AddWithValue("p_hashkey", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //p_titleid uuid,
                pgcom.Parameters.AddWithValue("p_titleid", NpgsqlTypes.NpgsqlDbType.Uuid, dataUser.TitleId);

                //p_clientuserid text,
                pgcom.Parameters.AddWithValue("p_clientuserid", NpgsqlTypes.NpgsqlDbType.Text, dataUser.UserId);

                //    p_client integer,
                pgcom.Parameters.AddWithValue("p_client", NpgsqlTypes.NpgsqlDbType.Integer, (int)dataUser.Client);

                //p_userlocale text,
                if (!string.IsNullOrWhiteSpace(dataUser.Locale))
                    pgcom.Parameters.AddWithValue("p_userlocale", NpgsqlTypes.NpgsqlDbType.Text, dataUser.Locale);
                else
                    pgcom.Parameters.AddWithValue("p_userlocale", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //    p_storynodename text,
                if (!string.IsNullOrWhiteSpace(dataUser.StoryNodeName))
                    pgcom.Parameters.AddWithValue("p_storynodename", NpgsqlTypes.NpgsqlDbType.Text,
                        dataUser.StoryNodeName);
                else
                    pgcom.Parameters.AddWithValue("p_storynodename", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //p_nodename text,
                if (!string.IsNullOrWhiteSpace(dataUser.CurrentNodeName))
                    pgcom.Parameters.AddWithValue("p_nodename", NpgsqlTypes.NpgsqlDbType.Text,
                        dataUser.CurrentNodeName);
                else
                    pgcom.Parameters.AddWithValue("p_nodename", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //    p_createdtime timestamp without time zone,
                pgcom.Parameters.AddWithValue("p_createdtime", NpgsqlTypes.NpgsqlDbType.Timestamp,
                    dataUser.CreatedTime);

                //    p_lastaccesseddate timestamp without time zone,
                pgcom.Parameters.AddWithValue("p_lastaccesseddate", NpgsqlTypes.NpgsqlDbType.Timestamp,
                    dataUser.LastAccessedDate);


                //    p_titlecrumbs jsonb,
                if ((dataUser.TitleState?.Any()).GetValueOrDefault(false))
                {
                    pgcom.Parameters.AddWithValue("p_titlecrumbs", NpgsqlTypes.NpgsqlDbType.Jsonb,
                        dataUser.GetTitleStateJson());
                }
                else
                {
                    pgcom.Parameters.AddWithValue("p_titlecrumbs", NpgsqlTypes.NpgsqlDbType.Jsonb, DBNull.Value);
                }


                //p_permanenttitlecrumbs jsonb)
                if ((dataUser.PermanentTitleState?.Any()).GetValueOrDefault(false))
                {
                    pgcom.Parameters.AddWithValue("p_permanenttitlecrumbs", NpgsqlTypes.NpgsqlDbType.Jsonb,
                        dataUser.GetPermanentTitleStateJson());
                }
                else
                {
                    pgcom.Parameters.AddWithValue("p_permanenttitlecrumbs", NpgsqlTypes.NpgsqlDbType.Jsonb,
                        DBNull.Value);
                }

                if (dataUser.IsGuest.HasValue)
                {
                    pgcom.Parameters.AddWithValue("p_isguest", NpgsqlTypes.NpgsqlDbType.Boolean,
                        dataUser.IsGuest.Value);
                }
                else
                {
                    pgcom.Parameters.AddWithValue("p_isguest", NpgsqlTypes.NpgsqlDbType.Boolean,
                        DBNull.Value);
                }

                Stopwatch dbTime = new Stopwatch();
                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();
                    await pgcom.ExecuteNonQueryAsync();
                }
                finally
                {
                    dbTime.Stop();
                    pgcon.Close();
                }

                dbTime.Stop();
                _logger.LogInformation($"UpsertUserPhoneConsent database time: {dbTime.ElapsedMilliseconds}");


            }



        }


        public async Task UpsertPhoneConsentAsync(
            UserPhoneConsent phoneConsent)
        {

            if (phoneConsent == null)
                throw new ArgumentNullException(nameof(phoneConsent));

            if (!phoneConsent.Id.HasValue)
                throw new ArgumentNullException(nameof(phoneConsent), "Id property must have a value");

            if (phoneConsent.PhoneId == default(Guid))
                throw new ArgumentNullException(nameof(phoneConsent), "PhoneId property must be set to a valid guid");

            if (phoneConsent.TitleClientUserId == default(Guid))
                throw new ArgumentNullException(nameof(phoneConsent), "TitleClientUserId property must be set to a valid guid");

            if (phoneConsent.EngineRequestId == default(Guid))
                throw new ArgumentNullException(nameof(phoneConsent), "EngineRequestId property must be set to a valid guid");

            if (phoneConsent.TitleVersionId == default(Guid))
                throw new ArgumentNullException(nameof(phoneConsent), "TitleVersionId property must be set to a valid guid");

            if (!phoneConsent.SmsConsentDate.HasValue)
                throw new ArgumentNullException(nameof(phoneConsent), "SmsConsentDate property must be set");

            if (phoneConsent.SmsConsentDate.Value.Equals(default(DateTime)))
                throw new ArgumentNullException(nameof(phoneConsent), "SmsConsentDate property must be set to a valid date");

            if (string.IsNullOrWhiteSpace(phoneConsent.Name))
                throw new ArgumentNullException(nameof(phoneConsent), "Name property cannot be null or empty");



            using (NpgsqlConnection pgcon = (NpgsqlConnection)this.Database.GetDbConnection())
            {


                NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.addupdatesmsconsent", pgcon)
                {
                    CommandType = CommandType.StoredProcedure
                };


                // p_id
                pgcom.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Uuid, phoneConsent.Id.Value);

                // p_titleclientuserid uuid
                pgcom.Parameters.AddWithValue("p_titleclientuserid", NpgsqlTypes.NpgsqlDbType.Uuid,
                    phoneConsent.TitleClientUserId);

                //  p_phoneid uuid,
                pgcom.Parameters.AddWithValue("p_phoneid", NpgsqlTypes.NpgsqlDbType.Uuid, phoneConsent.PhoneId);

                //  p_titleversionid uuid,
                pgcom.Parameters.AddWithValue("p_titleversionid", NpgsqlTypes.NpgsqlDbType.Uuid,
                    phoneConsent.TitleVersionId);

                //  p_name text, 
                pgcom.Parameters.AddWithValue("p_name", NpgsqlTypes.NpgsqlDbType.Text, phoneConsent.Name);

                //  p_isconsentgranted boolean,
                pgcom.Parameters.AddWithValue("p_isconsentgranted", NpgsqlDbType.Boolean,
                    phoneConsent.IsSmsConsentGranted);

                //   p_enginerequestid uuid,
                pgcom.Parameters.AddWithValue("p_enginerequestid", NpgsqlTypes.NpgsqlDbType.Uuid,
                    phoneConsent.EngineRequestId);

                //p_smsconsentdate timestamp without time zone)
                pgcom.Parameters.AddWithValue("p_smsconsentdate", NpgsqlTypes.NpgsqlDbType.Timestamp,
                    phoneConsent.SmsConsentDate);


                Stopwatch dbTime = new Stopwatch();
                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();
                    await pgcom.ExecuteNonQueryAsync();
                }
                finally
                {
                    dbTime.Stop();
                    pgcon.Close();
                }

                dbTime.Stop();
                _logger.LogInformation($"UpsertUserPhoneConsent database time: {dbTime.ElapsedMilliseconds}");
            }


        }


        public async Task AddOutboundMessageLog(OutboundMessageLogEntry messageLogEntry)
        {


            if (messageLogEntry == null)
                throw new ArgumentNullException(nameof(messageLogEntry));

            if (messageLogEntry.Id == default(Guid))
                throw new ArgumentNullException(nameof(messageLogEntry), "Id property be a valid value");

            if (messageLogEntry.OutboundMessageId == default(Guid))
                throw new ArgumentNullException(nameof(messageLogEntry), "OutboundMessageId property be a valid value");

            if (messageLogEntry.LogTime == default(DateTime))
                messageLogEntry.LogTime = DateTime.UtcNow;


            string dbConnText = this.Database.GetDbConnection().ConnectionString;

            using (NpgsqlConnection pgcon = new NpgsqlConnection(dbConnText))
            {
                //id uuid NOT NULL,
                //    outboundmessageid uuid NOT NULL,
                //    isexception boolean,
                //httpstatus integer,
                //    extendedstatus text COLLATE pg_catalog."default",
                //status integer NOT NULL,
                //    logtime timestamp without time zone NOT NULL,
                //"OutboundSmsMessageId" uuid,
                //providersendduration integer,

                NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.addoutboundmessagelog", pgcon)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // p_id
                pgcom.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Uuid, messageLogEntry.Id);



                // p_outboundmessageid
                pgcom.Parameters.AddWithValue("p_outboundmessageid", NpgsqlTypes.NpgsqlDbType.Uuid,
                    messageLogEntry.OutboundMessageId);

                // p_isexception
                pgcom.Parameters.AddWithValue("p_isexception", NpgsqlTypes.NpgsqlDbType.Boolean,
                    messageLogEntry.IsException.GetValueOrDefault(false));

                // p_httpstatus
                if (messageLogEntry.HttpStatusCode.HasValue)
                    pgcom.Parameters.AddWithValue("p_httpstatus", NpgsqlTypes.NpgsqlDbType.Integer,
                        messageLogEntry.HttpStatusCode.Value);
                else
                    pgcom.Parameters.AddWithValue("p_httpstatus", NpgsqlTypes.NpgsqlDbType.Integer, DBNull.Value);

                // extendedstatus
                if (!string.IsNullOrWhiteSpace(messageLogEntry.ExtendedStatus))
                    pgcom.Parameters.AddWithValue("p_extendedstatus", NpgsqlTypes.NpgsqlDbType.Text,
                        messageLogEntry.ExtendedStatus);
                else
                    pgcom.Parameters.AddWithValue("p_extendedstatus", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_status
                pgcom.Parameters.AddWithValue("p_status", NpgsqlTypes.NpgsqlDbType.Integer,
                    (int)messageLogEntry.SendStatus);

                // p_providersendduration
                if (messageLogEntry.ProviderSendDuration.HasValue)
                    pgcom.Parameters.AddWithValue("p_providersendduration", NpgsqlTypes.NpgsqlDbType.Integer,
                        (int)messageLogEntry.ProviderSendDuration.Value);
                else
                    pgcom.Parameters.AddWithValue("p_providersendduration", NpgsqlTypes.NpgsqlDbType.Integer,
                        DBNull.Value);



                // p_logtime
                pgcom.Parameters.AddWithValue("p_logtime", NpgsqlTypes.NpgsqlDbType.Timestamp,
                    messageLogEntry.LogTime);



                Stopwatch dbTime = new Stopwatch();
                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();
                    await pgcom.ExecuteNonQueryAsync();
                }
                finally
                {
                    dbTime.Stop();
                    pgcon.Close();
                }

                _logger.LogInformation($"UpsertPhoneInfoAsync database time: {dbTime.ElapsedMilliseconds}");




            }


        }

        public async Task UpsertPhoneInfoAsync(DataPhone phoneInfo)
        {
            if (phoneInfo == null)
                throw new ArgumentNullException(nameof(phoneInfo));

            if (!phoneInfo.Id.HasValue)
                throw new ArgumentNullException(nameof(phoneInfo), "Id property must have a value");

            if (string.IsNullOrWhiteSpace(phoneInfo.PhoneNumber))
                throw new ArgumentNullException(nameof(phoneInfo), "Phone property cannot be null or empty");


            using (NpgsqlConnection pgcon = (NpgsqlConnection)this.Database.GetDbConnection())
            {

                NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.addupdatephonenumber", pgcon)
                {
                    CommandType = CommandType.StoredProcedure
                };


                string guidText = phoneInfo.Id.Value.ToString();


                // p_id
                pgcom.Parameters.AddWithValue("p_id", NpgsqlTypes.NpgsqlDbType.Uuid, phoneInfo.Id);

                // p_phonenumber
                pgcom.Parameters.AddWithValue("p_phonenumber", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.PhoneNumber);

                // p_type
                pgcom.Parameters.AddWithValue("p_phonetype", NpgsqlTypes.NpgsqlDbType.Integer, (int)phoneInfo.Type);

                // p_isverified
                pgcom.Parameters.AddWithValue("p_isverified", NpgsqlTypes.NpgsqlDbType.Boolean, phoneInfo.IsVerified);

                // p_nationalformat 
                if (!string.IsNullOrWhiteSpace(phoneInfo.NationalFormat))
                    pgcom.Parameters.AddWithValue("p_nationalformat", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.NationalFormat);
                else
                    pgcom.Parameters.AddWithValue("p_nationalformat", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //  p_cangetsmsmessage
                pgcom.Parameters.AddWithValue("p_cangetsmsmessage", NpgsqlDbType.Boolean, phoneInfo.CanGetSmsMessage);

                //  p_countrycode
                if (!string.IsNullOrWhiteSpace(phoneInfo.CountryCode))
                    pgcom.Parameters.AddWithValue("p_countrycode", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.CountryCode);
                else
                    pgcom.Parameters.AddWithValue("p_countrycode", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //p_carriercountrycode
                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierCountryCode))
                    pgcom.Parameters.AddWithValue("p_carriercountrycode", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.CarrierCountryCode);
                else
                    pgcom.Parameters.AddWithValue("p_carriercountrycode", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_carriernetworkcode
                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierNetworkCode))
                    pgcom.Parameters.AddWithValue("p_carriernetworkcode", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.CarrierNetworkCode);
                else
                    pgcom.Parameters.AddWithValue("p_carriernetworkcode", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_carriername 
                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierName))
                    pgcom.Parameters.AddWithValue("p_carriername", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.CarrierName);
                else
                    pgcom.Parameters.AddWithValue("p_carriername", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_carriererrorcode 
                if (!string.IsNullOrWhiteSpace(phoneInfo.CarrierErrorCode))
                    pgcom.Parameters.AddWithValue("p_carriererrorcode", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.CarrierErrorCode);
                else
                    pgcom.Parameters.AddWithValue("p_carriererrorcode", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                //  p_url
                if (!string.IsNullOrWhiteSpace(phoneInfo.Url))
                    pgcom.Parameters.AddWithValue("p_url", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.Url);
                else
                    pgcom.Parameters.AddWithValue("p_url", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_registeredname
                if (!string.IsNullOrWhiteSpace(phoneInfo.RegisteredName))
                    pgcom.Parameters.AddWithValue("p_registeredname", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.RegisteredName);
                else
                    pgcom.Parameters.AddWithValue("p_registeredname", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_registeredtype
                if (!string.IsNullOrWhiteSpace(phoneInfo.RegisteredType))
                    pgcom.Parameters.AddWithValue("p_registeredtype", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.RegisteredType);
                else
                    pgcom.Parameters.AddWithValue("p_registeredtype", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_registerederrorcode
                if (!string.IsNullOrWhiteSpace(phoneInfo.RegisteredErrorCode))
                    pgcom.Parameters.AddWithValue("p_registerederrorcode", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.RegisteredErrorCode);
                else
                    pgcom.Parameters.AddWithValue("p_registerederrorcode", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);


                // p_phoneservice
                if (!string.IsNullOrWhiteSpace(phoneInfo.PhoneService))
                    pgcom.Parameters.AddWithValue("p_phoneservice", NpgsqlTypes.NpgsqlDbType.Text, phoneInfo.PhoneService);
                else
                    pgcom.Parameters.AddWithValue("p_phoneservice", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                // p_systemstatus
                pgcom.Parameters.AddWithValue("p_systemstatus", NpgsqlTypes.NpgsqlDbType.Integer, (int)phoneInfo.SystemStatus);

                // p_createdate
                pgcom.Parameters.AddWithValue("p_createdate", NpgsqlTypes.NpgsqlDbType.Timestamp, phoneInfo.CreateDate);
                Stopwatch dbTime = new Stopwatch();
                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();
                    await pgcom.ExecuteNonQueryAsync();
                }
                finally
                {
                    dbTime.Stop();
                    pgcon.Close();
                }

                _logger.LogInformation($"UpsertPhoneInfoAsync database time: {dbTime.ElapsedMilliseconds}");

            }




        }

        public async Task LogEngineRequestAsync(Guid? engineUserId, Guid engineSessionId, Guid engineRequestId, string sessionId, string requestId, Guid deploymentId,
                     string userId, string locale, string intentName,
                     Dictionary<string, string> slots, long processDuration, DateTime? startTime, DateTime? actionTime, StoryRequestType? requestType, string preNodeActionLog, string mappedNode, string postNodeActionLog,
                     YesNoMaybeEnum? canFulfill, Dictionary<string, SlotCanFulFill> slotFulfillment, float? intentConfidence, string rawText, Dictionary<string, string> requestAttributes, Dictionary<string, string> sessionAttributes,
                     bool? isFirstSession, string requestBodyText, string responseBodyText, string engineErrorText, string responseConversionError, bool? isGuest)
        {

            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentNullException(nameof(sessionId));

            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentNullException(nameof(requestId));


            using (NpgsqlConnection pgcon = (NpgsqlConnection)this.Database.GetDbConnection())
            {

                NpgsqlCommand pgcom = new NpgsqlCommand("whetstone.addintentaction", pgcon)
                {
                    CommandType = CommandType.StoredProcedure
                };



                if (engineUserId.HasValue)
                    pgcom.Parameters.AddWithValue("engineuserid", NpgsqlTypes.NpgsqlDbType.Uuid, engineUserId);
                else
                    pgcom.Parameters.AddWithValue("engineuserid", NpgsqlTypes.NpgsqlDbType.Uuid, DBNull.Value);


                pgcom.Parameters.AddWithValue("sessionkey", NpgsqlTypes.NpgsqlDbType.Uuid, engineSessionId);

                pgcom.Parameters.AddWithValue("enginerequestid", NpgsqlTypes.NpgsqlDbType.Uuid, engineRequestId);

                pgcom.Parameters.AddWithValue("requestid", requestId);

                pgcom.Parameters.AddWithValue("sessionid", sessionId);

                pgcom.Parameters.AddWithValue("deploymentkeyid", NpgsqlTypes.NpgsqlDbType.Uuid, deploymentId);

                if (!string.IsNullOrWhiteSpace(userId))
                    pgcom.Parameters.AddWithValue("userid", NpgsqlTypes.NpgsqlDbType.Text, userId);
                else
                    pgcom.Parameters.AddWithValue("userid", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(locale))
                    pgcom.Parameters.AddWithValue("locale", NpgsqlTypes.NpgsqlDbType.Varchar, 10, locale);
                else
                    pgcom.Parameters.AddWithValue("locale", NpgsqlTypes.NpgsqlDbType.Varchar, 10, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(intentName))
                    pgcom.Parameters.AddWithValue("intentname", NpgsqlTypes.NpgsqlDbType.Text, intentName);
                else
                    pgcom.Parameters.AddWithValue("intentname", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(preNodeActionLog))
                    pgcom.Parameters.AddWithValue("prenodeactionlog", NpgsqlTypes.NpgsqlDbType.Text, preNodeActionLog);
                else
                    pgcom.Parameters.AddWithValue("prenodeactionlog", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(mappedNode))
                    pgcom.Parameters.AddWithValue("mappednode", NpgsqlTypes.NpgsqlDbType.Text, mappedNode);
                else
                    pgcom.Parameters.AddWithValue("mappednode", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(postNodeActionLog))
                    pgcom.Parameters.AddWithValue("postnodeactionlog", NpgsqlTypes.NpgsqlDbType.Text, postNodeActionLog);
                else
                    pgcom.Parameters.AddWithValue("postnodeactionlog", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);


                if (requestType.HasValue)
                    pgcom.Parameters.AddWithValue("requesttype", NpgsqlTypes.NpgsqlDbType.Integer, (int)requestType);
                else
                    pgcom.Parameters.AddWithValue("requesttype", NpgsqlTypes.NpgsqlDbType.Integer, DBNull.Value);

                if (actionTime.HasValue)
                    pgcom.Parameters.AddWithValue("selectiontime", NpgsqlTypes.NpgsqlDbType.Timestamp, actionTime);
                else
                    pgcom.Parameters.AddWithValue("selectiontime", NpgsqlTypes.NpgsqlDbType.Timestamp, DBNull.Value);


                if (startTime.HasValue)
                    pgcom.Parameters.AddWithValue("starttime", NpgsqlTypes.NpgsqlDbType.Timestamp, startTime);
                else
                    pgcom.Parameters.AddWithValue("starttime", NpgsqlTypes.NpgsqlDbType.Timestamp, DBNull.Value);


                if (slots != null)
                    pgcom.Parameters.AddWithValue("slots", NpgsqlTypes.NpgsqlDbType.Hstore, slots);
                else
                    pgcom.Parameters.AddWithValue("slots", NpgsqlTypes.NpgsqlDbType.Hstore, DBNull.Value);


                pgcom.Parameters.AddWithValue("processduration", NpgsqlTypes.NpgsqlDbType.Integer, processDuration);

                if (canFulfill.HasValue)
                    pgcom.Parameters.AddWithValue("canfulfill", NpgsqlTypes.NpgsqlDbType.Integer, (int)canFulfill.Value);
                else
                    pgcom.Parameters.AddWithValue("canfulfill", NpgsqlTypes.NpgsqlDbType.Integer, DBNull.Value);


                if ((slotFulfillment?.Keys?.Any()).GetValueOrDefault(false))
                {
                    string slotfulfillText = JsonConvert.SerializeObject(slotFulfillment);

                    pgcom.Parameters.AddWithValue("fulfillmentslots", NpgsqlTypes.NpgsqlDbType.Jsonb, slotfulfillText);
                }
                else
                    pgcom.Parameters.AddWithValue("fulfillmentslots", NpgsqlTypes.NpgsqlDbType.Jsonb, DBNull.Value);


                if (isFirstSession.HasValue)
                    pgcom.Parameters.AddWithValue("isfirstsession", NpgsqlTypes.NpgsqlDbType.Boolean, isFirstSession.Value);
                else
                    pgcom.Parameters.AddWithValue("isfirstsession", NpgsqlTypes.NpgsqlDbType.Boolean, DBNull.Value);


                if ((requestAttributes?.Keys?.Any()).GetValueOrDefault(false))
                    pgcom.Parameters.AddWithValue("requestattributes", NpgsqlTypes.NpgsqlDbType.Hstore, requestAttributes);
                else
                    pgcom.Parameters.AddWithValue("requestattributes", NpgsqlTypes.NpgsqlDbType.Hstore, DBNull.Value);

                if ((sessionAttributes?.Keys?.Any()).GetValueOrDefault(false))
                    pgcom.Parameters.AddWithValue("sessionattributes", NpgsqlTypes.NpgsqlDbType.Hstore, sessionAttributes);
                else
                    pgcom.Parameters.AddWithValue("sessionattributes", NpgsqlTypes.NpgsqlDbType.Hstore, DBNull.Value);


                if (intentConfidence.HasValue)
                    pgcom.Parameters.AddWithValue("intentconfidence", NpgsqlTypes.NpgsqlDbType.Real, intentConfidence.Value);
                else
                    pgcom.Parameters.AddWithValue("intentconfidence", NpgsqlTypes.NpgsqlDbType.Real, DBNull.Value);


                if (!string.IsNullOrWhiteSpace(rawText))
                    pgcom.Parameters.AddWithValue("rawtext", NpgsqlTypes.NpgsqlDbType.Text, rawText);
                else
                    pgcom.Parameters.AddWithValue("rawtext", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);


                if (!string.IsNullOrWhiteSpace(requestBodyText))
                    pgcom.Parameters.AddWithValue("requestbodytext", NpgsqlTypes.NpgsqlDbType.Text, requestBodyText);
                else
                    pgcom.Parameters.AddWithValue("requestbodytext", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(responseBodyText))
                    pgcom.Parameters.AddWithValue("responsebodytext", NpgsqlTypes.NpgsqlDbType.Text, responseBodyText);
                else
                    pgcom.Parameters.AddWithValue("responsebodytext", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);


                if (!string.IsNullOrWhiteSpace(engineErrorText))
                    pgcom.Parameters.AddWithValue("engineerrortext", NpgsqlTypes.NpgsqlDbType.Text, engineErrorText);
                else
                    pgcom.Parameters.AddWithValue("engineerrortext", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);

                if (!string.IsNullOrWhiteSpace(responseConversionError))
                    pgcom.Parameters.AddWithValue("responseconvtext", NpgsqlTypes.NpgsqlDbType.Text, responseConversionError);
                else
                    pgcom.Parameters.AddWithValue("responseconvtext", NpgsqlTypes.NpgsqlDbType.Text, DBNull.Value);


                if (isGuest.HasValue)
                    pgcom.Parameters.AddWithValue("isguest", NpgsqlTypes.NpgsqlDbType.Boolean, isGuest.Value);
                else
                    pgcom.Parameters.AddWithValue("isguest", NpgsqlTypes.NpgsqlDbType.Boolean, DBNull.Value);

                Stopwatch dbTime = new Stopwatch();


                try
                {
                    dbTime.Start();
                    await pgcon.OpenAsync();

                    if (!pgcom.IsPrepared)
                        await pgcom.PrepareAsync();

                    await pgcom.ExecuteNonQueryAsync();


                }
                finally
                {
                    dbTime.Stop();
                    pgcon.Close();
                }

                _logger.LogInformation($"LogEngineRequestAsync database time: {dbTime.ElapsedMilliseconds}");
            }
        }


        public async Task SaveChangesAsync()
        {
            await base.SaveChangesAsync();
        }

        public async Task CreateUserAccountAsync(DataUser user, string accountName)
        {

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            try
            {

                DataGroup defaultGroup = new DataGroup
                {
                    Name = "Project Administrators",
                    Description = "Grants all project rights"
                };

                defaultGroup.GroupRoleXRefs = new List<DataGroupRoleXRef>
                {
                    new DataGroupRoleXRef
                    {
                        Group = defaultGroup,
                        RoleId = EngineRoles.ProjectAdministrator
                    }
                };

                // Create a new organization for the user
                DataOrganization org = new DataOrganization
                {
                    IsEnabled = true,
                    Description = "default organization",
                    Name = accountName,
                    SubscriptionLevelId = ServiceLevels.FreeTier,
                    Groups = new List<DataGroup>()
                };


                org.Groups.Add(defaultGroup);


                user.GroupXRefs = new List<DataUserGroupXRef>
                {
                    new DataUserGroupXRef
                    {
                        Group = defaultGroup,
                        User = user
                    }
                };


                this.Organizations.Add(org);
                this.Users.Add(user);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PostgresException postEx)
                {

                    if (postEx.SqlState.Equals(PostgresErrorCodes.ForeignKeyViolation))
                    {
                        throw new DuplicateKeyException("Duplicate key error while adding new user");
                    }

                    throw postEx;

                }

                throw;

            }


        }
    }
}
