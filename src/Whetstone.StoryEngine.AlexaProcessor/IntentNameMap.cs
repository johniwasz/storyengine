using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Whetstone.StoryEngine;

namespace Whetstone.StoryEngine.AlexaProcessor
{ 
    public static class IntentNameMap
    {


        private static readonly Lazy<BiDictionary<string, string>> _intentMap = new Lazy<BiDictionary<string, string>>(
            () =>
            {
                BiDictionary<string, string> retDict = new BiDictionary<string, string>();

                retDict.Add("YesIntent", "AMAZON.YesIntent");
                retDict.Add("NoIntent", "AMAZON.NoIntent");
                retDict.Add("RepeatIntent", "AMAZON.RepeatIntent");
                retDict.Add("ResumeIntent", "AMAZON.ResumeIntent");
                retDict.Add("RestartIntent", "AMAZON.StartOverIntent");
                retDict.Add("StopIntent", "AMAZON.StopIntent");
                return retDict;
            }, 
            LazyThreadSafetyMode.ExecutionAndPublication  
             
            );

        
        public static string GetAppIntent(string amazonIntent)
        {

            BiDictionary<string, string> amazonMap = _intentMap.Value;
            string intentName = null;

           var appIntentNames = amazonMap.GetBySecond(amazonIntent);

            if (appIntentNames != null)
            {
                if (appIntentNames.Count > 0)
                    intentName = appIntentNames[0];
            }
       
            return intentName;
        }


        public static string GetAmazonIntent(string appIntent)
        {

            BiDictionary<string, string> amazonMap = _intentMap.Value;
            string intentName = null;

            var appIntentNames = amazonMap.GetByFirst(appIntent);

            if (appIntentNames != null)
            {
                if (appIntentNames.Count > 0)
                    intentName = appIntentNames[0];
            }

            return intentName;
        }



    }
}
