using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Npgsql;
using Org.BouncyCastle.Crypto.Tls;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class UserDataRepository : UserRepositoryBase, IStoryUserRepository
    {

        private readonly IUserContextRetriever _contextRetriever;

        private readonly ILogger<UserDataRepository> _dataLogger;



        public UserDataRepository(IUserContextRetriever contextRetriever, ILogger<UserDataRepository> logger)
        {

            _contextRetriever = contextRetriever ?? throw new ArgumentNullException($"{nameof(contextRetriever)}");

            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }




        protected async Task<DataTitleClientUser> CreateUserAsync(StoryRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));



            DataTitleClientUser user = BootstrapUser(request);


            Guid? titleId = request.SessionContext?.TitleVersion?.TitleId;


            Guid foundTitleId = default;


         

            if (titleId.HasValue)
            {
                foundTitleId = titleId.Value;
            }
            else
            {
                using (var dbContext = await _contextRetriever.GetUserDataContextAsync())
                {
                    var foundTitle = await dbContext.Titles.Join(dbContext.TitleVersions,
                            t => t.Id,
                            tv => tv.TitleId,
                            (t, tv) => new {TitleId = t.Id, VersionId = tv.Id})
                        .Join(dbContext.TitleVersionDeployments,
                            ttv => ttv.VersionId,
                            tvd => tvd.VersionId,
                            (ttv, tvd) => new
                            {
                                DeploymentId = tvd.Id, ApplicationId = tvd.ClientIdentifier, ClientType = tvd.Client,
                                TitleId = ttv.TitleId
                            })
                        .Where(x => request.ApplicationId.ToLower().Equals(x.ApplicationId.ToLower()) &&
                                    request.Client.Equals(x.ClientType)).Select(x => x.TitleId).FirstOrDefaultAsync();


                    if (foundTitle.HasValue)
                        foundTitleId = foundTitle.Value;

                }
            }

            user.TitleId =  foundTitleId;


            // Add a new user to the database.
            using (var dbContext = await _contextRetriever.GetUserDataContextAsync())
            {
                dbContext.ClientUsers.Add(user);
                await dbContext.SaveChangesAsync();

            }

            return user;
        }






        public async Task<DataTitleClientUser> GetUserAsync(StoryRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

 
            Client clientType = request.Client;
            Guid? titleId = null;
            string titleShortName = null;


            if (request.SessionContext?.TitleVersion != null)
            {
                TitleVersion titleVer = request.SessionContext.TitleVersion;
                titleId = titleVer.TitleId;
                titleShortName = titleVer.ShortName;

            }

            DataTitleClientUser retUser = null;
        

            using (IUserDataContext context =await _contextRetriever.GetUserDataContextAsync())
            {
                DataTitleClientUser foundUser = null;
                if (titleId.HasValue && !string.IsNullOrWhiteSpace(titleShortName))
                {
                    foundUser = await context.ClientUsers.SingleOrDefaultAsync(x => x.TitleId.Equals(titleId) &&
                                                                                        x.UserId.Equals(
                                                                                            request.UserId) &&
                                                                                        x.Client == clientType);

                 
                }
                else
                {

                    var foundTitleUser = await context.Titles.Join(context.TitleVersions,
                            t => t.Id,
                            tv => tv.TitleId,
                            (t, tv) => new { TitleId = t.Id, VersionId = tv.Id, ShortName = t.ShortName })
                        .Join(context.TitleVersionDeployments,

                            ttv => ttv.VersionId,
                            tvd => tvd.VersionId,
                            (ttv, tvd) => new
                                { DeploymentId = tvd.Id, ApplicationId = tvd.ClientIdentifier, ClientType = tvd.Client, TitleId = ttv.TitleId, ShortName = ttv.ShortName })

                        .Join(context.ClientUsers,
                            tvd => tvd.TitleId,
                            cu => cu.TitleId,
                            (tvd, cu) => new { TitleId = tvd.TitleId, ApplicationId = tvd.ApplicationId, ClientType =tvd.ClientType, ClientUser = cu, ShortName =tvd.ShortName})

                        .Where(x => request.ApplicationId.ToLower().Equals(x.ApplicationId.ToLower()) &&
                                    request.Client.Equals(x.ClientType)).FirstOrDefaultAsync();


                    if (foundTitleUser != null)
                    {
                        foundUser = foundTitleUser.ClientUser;
                    }

                }

                if (foundUser == null)
                {
                    retUser = await CreateUserAsync(request);
                }
                else
                    retUser = foundUser;
            }

            return retUser;
        }




        public async Task SaveUserAsync(DataTitleClientUser user)
        {
            if (user == null)
                throw new ArgumentException($"{nameof(user)} cannot be null");

            if(user.LastAccessedDate == default(DateTime))
                user.LastAccessedDate = DateTime.UtcNow;


            try
            {

                using (var userContext = await _contextRetriever.GetUserDataContextAsync())
                {
                    await userContext.UpsertTitleUsertAsync( user);
                }
            }
            catch (DbUpdateException entityEx)
            {
                if (entityEx.InnerException != null)
                {
                    if (entityEx.InnerException is PostgresException)
                    {
                        PostgresException postEx = (PostgresException) entityEx.InnerException;

                        if (postEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY))
                        {

                            _dataLogger.LogError(postEx,
                                $"Unexpected duplicate key error adding user with title {user.TitleId}, client {user.Client}, clientid {user.UserId} already exists in database.");
                            throw;
                        }

                    }
                    else
                    {
                        throw;
                    }

                }
                else
                {
                    throw;
                }

            }
            catch (PostgresException postEx)
            {
                if (postEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY))
                {
                    user.Id = null;
                    _dataLogger.LogWarning(
                        $"User with title {user.TitleId}, client {user.Client}, clientid {user.UserId} already exists in database.");
                    // do not throw an exception. this is expected.
                }
                else
                {
                    _dataLogger.LogError(postEx, "Unexpected PostgreSQL exception adding user to the database.");
                    throw;
                }
            }
        }


        private async Task InsertNewUserAsync(DataTitleClientUser user)
        {
            if (user == null)
                throw new ArgumentNullException($"{nameof(user)}");

            using (var dbContext = await _contextRetriever.GetUserDataContextAsync())
            {
                    dbContext.ClientUsers.Add(user);
                    await dbContext.SaveChangesAsync();
            }
        }

        private async Task UpdateUserAsync(DataTitleClientUser user)
        {
           

            bool isUpdate = false;
            DataTitleClientUser foundUser = null;
            if (user == null)
                throw new ArgumentNullException($"{nameof(user)}");

            if (user.Id.HasValue)
            {
                _dataLogger.LogInformation($"Retrieving user by id {user.Id}");
    
                using (var dbContext = await _contextRetriever.GetUserDataContextAsync())
                {
                    foundUser = await dbContext.ClientUsers.FirstOrDefaultAsync(x => x.Id.Equals(user.Id));
                }
            }
            else
            {
                _dataLogger.LogInformation("Retrieving user by title: {@user}", user);

                using (var dbContext = await _contextRetriever.GetUserDataContextAsync())
                {
                    foundUser = await dbContext.ClientUsers
                        .Where(x => user.TitleId.Equals(x.TitleId) && user.Client.Equals(x.Client) &&
                                    user.UserId.Equals(x.UserId)).SingleOrDefaultAsync();
                }


            }


            if (foundUser == null)
            {
                string errorText =
                    $"Error updating user with title id {user.TitleId}, client {user.Client}, clientid {user.UserId}: user not found in database";
                _dataLogger.LogError(errorText);
                throw new Exception(errorText);
            }


            isUpdate = user.LastAccessedDate > foundUser.LastAccessedDate;

            if (isUpdate)
            {
           
                user.Id = foundUser.Id;
                _dataLogger.LogInformation($"Updating user by user id {user.Id}");

                try
                {
                    using (var dbContext = await _contextRetriever.GetUserDataContextAsync())
                    {
                        // The client user id, title id and client do not need to be updated.
                        dbContext.ClientUsers.Attach(user);
                        dbContext.Entry(user).Property(du => du.CurrentNodeName).IsModified = true;
                        dbContext.Entry(user).Property(du => du.Locale).IsModified = true;
                        dbContext.Entry(user).Property(du => du.LastAccessedDate).IsModified = true;
                        dbContext.Entry(user).Property("PermanentTitleStateJson").IsModified = true;
                        dbContext.Entry(user).Property("TitleStateJson").IsModified = true;
                        dbContext.Entry(user).Property(du => du.StoryNodeName).IsModified = true;
                        await dbContext.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    _dataLogger.LogError(ex, $"Error updating user id {user.Id}");
                    throw;
                }
            }
            else
            {
                _dataLogger.LogInformation($"User id {foundUser.Id} in database has date {foundUser.LastAccessedDate} later than the user record being updated {user.LastAccessedDate}");
            }
        }




    }
}
