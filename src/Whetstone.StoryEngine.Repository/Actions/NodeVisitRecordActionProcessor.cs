
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class NodeVisitRecordActionProcessor : INodeActionProcessor
    {


        private readonly ILogger<NodeVisitRecordActionProcessor> _dataLogger;

        private readonly ITitleReader _titleReader;

        public NodeVisitRecordActionProcessor(ITitleReader titleReader, ILogger<NodeVisitRecordActionProcessor> logger)
        {

            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));

            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

#pragma warning disable CS1998
        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
#pragma warning restore CS1998
        {
            if (req == null)
                throw new ArgumentNullException(nameof(req));

            if (req.SessionContext?.TitleVersion == null)
                throw new ArgumentNullException(nameof(req), "SessionContext.TitleVersion cannot be null");


            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));

            StringBuilder actionBuilder = new StringBuilder();

            bool isCrumbFound = false;
            string nodeName = actionData.ParentNodeName;

            if (string.IsNullOrWhiteSpace(nodeName))
                throw new ArgumentException("Parent node name on node visit action is null or blank");

            bool isPrivacyLoggingEnabled = await _titleReader.IsPrivacyLoggingEnabledAsync(req.SessionContext.TitleVersion);



            if (crumbs != null && crumbs.Any())
            {
                if (crumbs.FirstOrDefault(x =>
                {
                    if (x is NodeVisitRecord visitRecord)
                    {
                        return visitRecord.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase);
                    }

                    return false;

                }) is NodeVisitRecord crumbRecord)
                {
                    int visitCount = crumbRecord.VisitCount++;
                    isCrumbFound = true;

                    string message = isPrivacyLoggingEnabled ?
                         $"User visited node (redacted) (redacted) times" :
                         $"User visited node {nodeName} {visitCount} times";

                    actionBuilder.Append("NodeVisit: ");
                    actionBuilder.AppendLine(message);

                    _dataLogger.LogInformation(message);
                }
            }


            if (!isCrumbFound)
            {
                NodeVisitRecord visitRecord = new NodeVisitRecord() { VisitCount = 1, Name = nodeName };
                if (crumbs == null)
                    crumbs = new List<IStoryCrumb>();


                crumbs.Add(visitRecord);

                string message = isPrivacyLoggingEnabled ?
                     $"Recording first visit to node (redacted)"
                    : $"Recording first visit to node {nodeName}";

                actionBuilder.Append("NodeVisit: ");
                actionBuilder.AppendLine(message);


                _dataLogger.LogInformation(message);
            }


            return actionBuilder.ToString();
        }
    }
}
