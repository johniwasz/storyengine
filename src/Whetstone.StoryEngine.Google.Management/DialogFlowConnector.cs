using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Cloud.Dialogflow.V2;
using System.Diagnostics;

namespace Whetstone.StoryEngine.Google.Management
{
    public class DialogFlowConnector
    {
        public static int Create(string projectId,
                                 string displayName,
                                 string messageText,
                                 string[] trainingPhrasesParts)
        {
            var client = IntentsClient.Create();

            var text = new Intent.Types.Message.Types.Text();
            text.Text_.Add(messageText);

            var message = new Intent.Types.Message()
            {
                Text = text
            };

            var phraseParts = new List<Intent.Types.TrainingPhrase.Types.Part>();
            foreach (var part in trainingPhrasesParts)
            {
                phraseParts.Add(new Intent.Types.TrainingPhrase.Types.Part()
                {
                    Text = part
                });
            }

            var trainingPhrase = new Intent.Types.TrainingPhrase();
            trainingPhrase.Parts.AddRange(phraseParts);

            var intent = new Intent();
            intent.DisplayName = displayName;
            intent.Messages.Add(message);
            intent.TrainingPhrases.Add(trainingPhrase);

            var newIntent = client.CreateIntent(
                parent: new ProjectAgentName(projectId),
                intent: intent
            );

            Console.WriteLine($"Created Intent: {newIntent.Name}");

            return 0;
        }



        public static  void ListIntentsAsync()
        {
            //projects /< Project ID >
            // Create the service.

            ///  dialogflowmanager - 226118
            ///  

            //  { "installed":{ "client_id":"360207265510-ju2363v40p2119u2751lcjtv8g9ebqkb.apps.googleusercontent.com","project_id":"eye-of-the-elder-gods","auth_uri":"https://accounts.google.com/o/oauth2/auth","token_uri":"https://www.googleapis.com/oauth2/v3/token","auth_provider_x509_cert_url":"https://www.googleapis.com/oauth2/v1/certs","client_secret":"BlI2uDmYkernHXuIm-DAPbHz","redirect_uris":["urn:ietf:wg:oauth:2.0:oob","http://localhost"]


            var client = IntentsClient.Create();

            IntentsClient.Create();

            try
            {
                var intents = client.ListIntents(new ProjectAgentName("eye-of-the-elder-gods"));
                foreach (var intent in intents)
                {
                    Console.WriteLine($"Intent name: {intent.Name}");
                    Console.WriteLine($"Intent display name: {intent.DisplayName}");
                }
            }
            catch(Exception ex)
            {

                Debug.WriteLine(ex);

            }
         

            //ProjectsResource projResource = service.Projects;


            //var getRequest = projResource.GetAgent("projects/eye-of-the-elder-gods");


            //var getResult = await getRequest.ExecuteAsync();


            //var agentResource = projResource.Agent;
            

           // var intentsResource =  agentResource.Intents;

          //  intentsResource.List()

            //// Run the request.
            //Console.WriteLine("Executing a list request...");
            //var result = await service.

            //// Display the results.
            //if (result.Items != null)
            //{
            //    foreach (DirectoryList.ItemsData api in result.Items)
            //    {
            //        Console.WriteLine(api.Id + " - " + api.Title);
            //    }
            //}


        }

        //public static async Task<UserCredential> GetCredential()
        //{
        //    using (var stream = new FileStream(@"C:\Users\John\googlecloudconfig\dialogflowmanager-226118-28a0b4a35a80.json",
        //         FileMode.Open, FileAccess.Read))
        //    {
        //        string loginEmailAddress = "iwaszj@gmail.com";
        //        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //            GoogleClientSecrets.Load(stream).Secrets,
        //            new[] { DialogflowService.Scope.Dialogflow },
        //            loginEmailAddress, CancellationToken.None,
        //            new FileDataStore("GoogleAnalyticsApiConsole"));
        //    }
        //}

    }
}
