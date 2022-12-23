using Whetstone.StoryEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Data
{
    public abstract class UserRepositoryBase
    {

        /// <summary>
        /// Assigns default values to a new user. This does not save the user to any backing store.
        /// </summary>
        /// <param name="request">An engine request object processed through an Alexa or Google adapter.</param>
        /// <returns>An initialized data client user.</returns>
        public DataTitleClientUser BootstrapUser( StoryRequest request)
        {
            DateTime curTime = DateTime.UtcNow;
            DataTitleClientUser user = new DataTitleClientUser();

            // Assign a new engine user id. This is used downstream
            // This id is used for references to other entities, like phone confirmations
            // and SMS messaging.
            user.Id = Guid.NewGuid();

            // If the userId did not come in on the request, then use the engine's user id as
            // the client-facing identifier.
            // This is the case for users that are guests as indicated by Google. Specifically,
            // if IsGuest is true, then the request.UserId property coming in will be null.
            user.UserId = string.IsNullOrWhiteSpace(request.UserId) ? user.Id.ToString() : request.UserId;
            user.Client = request.Client;
            user.LastAccessedDate = request.RequestTime;
            user.CreatedTime = request.RequestTime;
            user.Locale = request.Locale;
            user.IsNew = true;
            user.IsGuest = request.IsGuest;
            user.TitleState = new List<Models.Tracking.IStoryCrumb>();
            user.PermanentTitleState = new List<Models.Tracking.IStoryCrumb>();


            if (request.SessionContext.TitleVersion.TitleId.HasValue)
                user.TitleId = request.SessionContext.TitleVersion.TitleId.Value;

            user.HashKey = user.GenerateHashKey();


            return user;
        }


    }
}
