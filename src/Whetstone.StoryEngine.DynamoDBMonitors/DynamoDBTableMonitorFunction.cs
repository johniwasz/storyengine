using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Whetstone.StoryEngine.DynamoDBMonitors
{
    public class DynamoDBTableMonitorFunction : EngineLambdaBase
    {

        private const string SORT_KEY_NAME = "sortKey";

        internal enum PropagationType
        {
            Unknown = 0,
            User = 1,
            Phone = 2,
        }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public DynamoDBTableMonitorFunction() : base()
        {
        }


        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public async Task SyncUserRecords(DynamoDBEvent request, ILambdaContext context)
        {
            if (request == null)
                throw new ArgumentNullException($"{nameof(request)}");



            var userRepFunc = this.Services.GetRequiredService<Func<UserRepositoryType, IStoryUserRepository>>();
            IStoryUserRepository dbRep = userRepFunc(UserRepositoryType.Database);

            IUserContextRetriever userContextRetriever = this.Services.GetRequiredService<IUserContextRetriever>();

            ILogger<DynamoDBTableMonitorFunction> dbLogger = Services.GetService<ILogger<DynamoDBTableMonitorFunction>>();

            List<Exception> exList = new List<Exception>();
            foreach (var streamItem in request.Records.OrderBy(x => x.Dynamodb.SequenceNumber))
            {
                try
                {
                    dbLogger.LogInformation($"Processing sequence number {streamItem.Dynamodb.SequenceNumber}");

                    if (streamItem.Dynamodb.StreamViewType == StreamViewType.NEW_AND_OLD_IMAGES)
                    {
                        Dictionary<string, AttributeValue> newStreamImage = streamItem.Dynamodb.NewImage;

                        PropagationType propType = PropagationType.Unknown;
                        // Route the inbound object to the right handling logic.
                        if (newStreamImage.ContainsKey(SORT_KEY_NAME))
                        {
                            string keyVal = newStreamImage[SORT_KEY_NAME].S;
                            if (keyVal.Equals(DataTitleClientUser.SORT_KEY_VALUE))
                                propType = PropagationType.User;
                            else if (keyVal.Equals(DataPhone.SORT_KEY_VALUE))
                                propType = PropagationType.Phone;
                        }

                        switch (propType)
                        {
                            case PropagationType.User:
                                await ProcessUserUpdateAsync(newStreamImage, dbRep, streamItem.Dynamodb.SequenceNumber);
                                break;
                            case PropagationType.Phone:
                                await ProcessPhoneUpdateAsync(newStreamImage, userContextRetriever,
                                    streamItem.Dynamodb.SequenceNumber);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exList.Add(ex);
                }
            }

            if (exList.Count > 0)
                throw new AggregateException(exList);

        }

        private async Task ProcessUserPhoneConsent(Dictionary<string, AttributeValue> newStreamImage, string sequenceNumber)
        {
            UserPhoneConsent consentInfo = null;

            string consentIdText = newStreamImage["id"].S;


            ILogger<DynamoDBTableMonitorFunction> dbLogger = Services.GetService<ILogger<DynamoDBTableMonitorFunction>>();

            try
            {
                consentInfo = (UserPhoneConsent)newStreamImage;

            }
            catch (Exception ex)
            {
                dbLogger.LogError(ex, $"Error deserializing consent record with hashKey {consentIdText} and cannot save to database");

            }

            if (consentInfo != null)
            {
                // if the user coming in from dynamo db does not yet have a user id set,
                // then set the new user flag so that the dynamodb record is updated
                // with the database user id


                try
                {
                    dbLogger.LogInformation(
                        $"Saved consent with id {consentIdText} to database");

                }
                catch (Exception ex)
                {
                    dbLogger.LogError(ex, $"Error saving consent record to database with id {consentIdText}");
                    throw;
                }


            }
            else
            {
                dbLogger.LogInformation(
                    $"New image for sequence id {sequenceNumber} is empty. No message to process.");
            }
        }

        private async Task ProcessPhoneUpdateAsync(Dictionary<string, AttributeValue> newStreamImage, IUserContextRetriever userContextRetriever, string sequenceNum)
        {
            DataPhone phoneInfo = null;

            string phoneIdText = newStreamImage["id"].S;

            ILogger<DynamoDBTableMonitorFunction> dbLogger = Services.GetService<ILogger<DynamoDBTableMonitorFunction>>();

            try
            {
                phoneInfo = (DataPhone)newStreamImage;

            }
            catch (Exception ex)
            {
                dbLogger.LogError(ex, $"Error deserializing phone record with hashKey {phoneIdText} and cannot save to database");
                throw;
            }

            if (phoneInfo != null)
            {
                // if the user coming in from dynamo db does not yet have a user id set,
                // then set the new user flag so that the dynamodb record is updated
                // with the database user id


                try
                {


                    using (var userContext = await userContextRetriever.GetUserDataContextAsync())
                    {
                        await userContext.UpsertPhoneInfoAsync(phoneInfo);
                    }

                    dbLogger.LogInformation(
                        $"Saved phone number with hash key {phoneIdText} to database");

                }
                catch (Exception ex)
                {
                    dbLogger.LogError(ex, $"Error saving phone record to database with id {phoneIdText}");
                    throw;
                }


            }
            else
            {
                dbLogger.LogInformation(
                    $"New image for sequence id {sequenceNum} is empty. Nothing to process.");
            }

        }

        private async Task ProcessUserUpdateAsync(Dictionary<string, AttributeValue> newStreamImage, IStoryUserRepository dbRep, string sequenceNum)
        {


            string id = newStreamImage["id"].S;

            ILogger<DynamoDBTableMonitorFunction> dbLogger = Services.GetService<ILogger<DynamoDBTableMonitorFunction>>();

            DataTitleClientUser titleClient = null;

            try
            {
                titleClient = (DataTitleClientUser)newStreamImage;

            }
            catch (Exception ex)
            {
                dbLogger.LogError(ex, $"Error deserializing user record with hashKey {id} and cannot save to database");
                throw;
            }


            if (titleClient != null)
            {

                try
                {

                    Stopwatch saveTime = new Stopwatch();
                    saveTime.Start();
                    await dbRep.SaveUserAsync(titleClient);
                    dbLogger.LogInformation($"Time saving user {titleClient.Id} to database: {saveTime.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    dbLogger.LogError(ex, "Error saving user record to database");
                    throw;
                }

                dbLogger.LogInformation(
                    $"Saved new user with id {titleClient.Id.Value} to database");

            }
            else
            {
                dbLogger.LogInformation(
                    $"New image for sequence id {sequenceNum} is empty. Nothing to process.");
            }

        }


        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
        {


            BootstrapConfig bootConfig = Configuration.Get<BootstrapConfig>();

            DatabaseConfig dbConfig = bootConfig.DatabaseSettings;

            DataBootstrapping.ConfigureDatabaseService(services, dbConfig);

            services.AddTransient<UserDataRepository>();

            services.AddSingleton<Func<UserRepositoryType, IStoryUserRepository>>(serviceProvider => handlerKey =>
            {
                IStoryUserRepository retUserRep = null;
                switch (handlerKey)
                {
                    case UserRepositoryType.Database:
                        retUserRep = serviceProvider.GetService<UserDataRepository>();
                        break;
                    case UserRepositoryType.DynamoDB:
                        retUserRep = serviceProvider.GetService<DynamoDBUserRepository>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return retUserRep;
            });

            base.ConfigureServices(services, config);
        }


    }
}
