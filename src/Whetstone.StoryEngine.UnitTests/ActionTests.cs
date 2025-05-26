using Amazon;
using System;
using System.Collections.Generic;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.UnitTests
{
    public class ActionTests
    {
        private void SetEnvironmentVariables()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");


            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"dev-cache-sanjtest.prgrxr.ng.0001.use1.cache.amazonaws.com");


            //System.Environment.SetEnvironmentVariable("ENCRYPTIONKEYNAME", "alias/ExternalSystemSecretsKey");

            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"localhost");

        }



        public static (StoryRequest, List<IStoryCrumb>) BuildStoryRequest(string titleId, string phoneNumber, string phoneSlot)
        {
            StoryRequest req = new StoryRequest();
            string strGuid = Guid.NewGuid().ToString();
            req.SessionContext = new EngineSessionContext();

            req.SessionContext.TitleVersion = new TitleVersion();
            req.SessionContext.TitleVersion.ShortName = titleId;
            req.SessionContext.TitleVersion.Version = "1.0";
            req.SessionContext.TitleVersion.TitleId = Guid.NewGuid();
            req.SessionContext.EngineUserId = Guid.NewGuid();

            req.ApplicationId = "amzn1.ask.skill.2704cc00-6641-4530-a076-b65ed8a0b2d6";
            req.SessionId = "amzn1.echo-api.session.897f2b5d-0178-46f9-989d-9c547c3bfa56";
            req.UserId = "amzn1.ask.account.AEYRFEC6CZGACDAJ5TDUNKBZMVONI7I4DGBCLMILPQBTIH3QDMSF4WFXYRV27TJ7FMZA4CYCIBZXMXOYRUUNKQTBGYJHC4MAFFJU6SPQSKORUSDLREJTOQFVSYKONFL5BLFKIUK2NOXPJHFYEX57B46QR42FWR4FLVWHHN5GY3RIYT6PUGIATM3FUL4FZ62JIIFDA3NL2TB5IUQ";
            req.RequestId = "amzn1.echo-api.request." + strGuid;
            req.Locale = "en-US";

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();



            SelectedItem selItem = new SelectedItem();
            selItem.Name = phoneSlot;
            selItem.Value = phoneNumber;
            //  selItem.Value = "6105551212";
            // selItem.Value = "2065273556";
            crumbs.Add(selItem);



            return (req, crumbs);
        }






    }
}
