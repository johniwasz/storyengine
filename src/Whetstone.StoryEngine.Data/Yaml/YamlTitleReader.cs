using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.Yaml
{
    public class YamlTitleReader : ITitleReader
    {

        private readonly ITitleCacheRepository _cacheRep = null;

        private readonly ILogger _dataLogger;

        public YamlTitleReader(ITitleCacheRepository cacheRep, ILogger<YamlTitleReader> logger)
        {
            _cacheRep = cacheRep ?? throw new ArgumentNullException(nameof(cacheRep));
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<StoryTitle> GetByIdAsync(TitleVersion titleVersion)
        {
            string titleId = titleVersion.ShortName;
            StoryTitle title = null;

            try
            {
                title = await _cacheRep.GetStoryTitleAsync(titleVersion);

            }
            catch (Exception ex)
            {
                _dataLogger.LogError($"{titleId} and version {titleVersion.Version} not found: {ex}");
            }



            if (title == null)
            {
                string message = $"Title {titleVersion.ShortName} and version {titleVersion.Version} not found";
                throw new TitleNotFoundException(message);
            }
            return title;
        }

        public async Task ClearTitleAsync(TitleVersion titleVersion)
        {

            string titleId = titleVersion.ShortName;

            await _cacheRep.RemoveTitleVersionAsync(titleVersion, false);

            _dataLogger.LogInformation($"Removed title {titleId} from local storage");

        }

        public async Task<bool> IsPrivacyLoggingEnabledAsync(TitleVersion titleVersion)
        {


            string titleId = titleVersion.ShortName;

            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            var storyTitle = await GetByIdAsync(titleVersion);



            return storyTitle.EnablePrivacyLogging.GetValueOrDefault(false);

        }

        public async Task<List<SlotType>> GetSlotTypes(TitleVersion titleVersion)
        {

            string titleId = titleVersion.ShortName;

            List<SlotType> slotTypes;
            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            var storyTitle = await GetByIdAsync(titleVersion);

            slotTypes = storyTitle?.Slots;

            return slotTypes;
        }


        public async Task<Intent> GetIntentByNameAsync(TitleVersion titleVersion, string intentName)
        {
            if (titleVersion == null)
                throw new ArgumentNullException($"{nameof(titleVersion)} cannot be null or empty");


            string titleId = titleVersion.ShortName;

            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));


            if (string.IsNullOrWhiteSpace(intentName))
                throw new ArgumentNullException(nameof(intentName));

            var storyTitle = await GetByIdAsync(titleVersion);

            var intent = storyTitle?.Intents?.FirstOrDefault(x => x.Name.Equals(intentName, StringComparison.OrdinalIgnoreCase));


            if (intent != null)
            {
                intent.TitleId = storyTitle.Id;
                _dataLogger.LogInformation($"Returning intent {intentName} for title {titleId}");
            }
            else
            {
                _dataLogger.LogInformation($"Intent {intentName} for title {titleId} not found");
            }

            return intent;
        }


        public async Task<string> GetStartNodeNameAsync(TitleVersion titleVersion, bool isNewUser)
        {
            if (titleVersion == null)
                throw new ArgumentNullException($"{nameof(titleVersion)}");

            string titleId = titleVersion.ShortName;

            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            var storyTitle = await GetByIdAsync(titleVersion);

            string retNodeName;


            if (isNewUser)
            {
                retNodeName = storyTitle?.NewUserNodeName;
            }
            else
            {
                retNodeName = storyTitle?.ReturningUserNodeName;
            }

            return retNodeName;
        }


        public async Task<StoryPhoneInfo> GetPhoneInfoAsync(TitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException($"{nameof(titleVersion)}");

            string titleId = titleVersion.ShortName;


            StoryPhoneInfo retPhoneInfo = null;
            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            var storyTitle = await GetByIdAsync(titleVersion);

            if (storyTitle != null)
            {
                retPhoneInfo = storyTitle.PhoneInfo;
            }

            return retPhoneInfo;
        }


        public async Task<List<Intent>> GetIntentsAsync(TitleVersion titleVersion)
        {

            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            string titleId = titleVersion.ShortName;

            List<Intent> intents = null;
            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            var storyTitle = await GetByIdAsync(titleVersion);


            if (storyTitle != null)
            {
                intents = storyTitle.Intents;

            }

            return intents;
        }

        public async Task<StoryNode> GetNodeByNameAsync(TitleVersion titleVersion, string storyNodeName)
        {

            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            string titleId = titleVersion.ShortName;

            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));


            if (string.IsNullOrWhiteSpace(storyNodeName))
                throw new ArgumentNullException(nameof(storyNodeName));

            var storyTitle = await GetByIdAsync(titleVersion);


            var node = storyTitle?.Nodes?.FirstOrDefault(x => x.Name.Equals(storyNodeName, StringComparison.OrdinalIgnoreCase));

            if (node != null)
            {
                node.TitleId = storyTitle.Id;
                _dataLogger.LogDebug($"Returning node {storyNodeName} for title {titleVersion.ShortName} and version {titleVersion.Version}");
            }
            else
            {
                _dataLogger.LogError($"Node {storyNodeName} for title {titleVersion.ShortName} and version {titleVersion.Version} not found");
            }

            return node;
        }

        public async Task<StoryConditionBase> GetStoryConditionAsync(TitleVersion titleVersion, string conditionName)
        {
            if (titleVersion == null)
                throw new ArgumentNullException($"{nameof(titleVersion)}");

            var storyTitle = await GetByIdAsync(titleVersion);

            StoryConditionBase conditionBase = storyTitle?.Conditions?.FirstOrDefault(x =>
                x.Name.Equals(conditionName, StringComparison.OrdinalIgnoreCase));

            return conditionBase;
        }

        public async Task<ICollection<StoryNode>> GetNodesByTitleAsync(TitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            var storyTitle = await GetByIdAsync(titleVersion);

            var nodes = storyTitle.Nodes;

            foreach (var node in nodes)
            {
                node.TitleId = storyTitle.Id;
            }

            return storyTitle.Nodes;
        }

        public async Task<StoryNode> GetBadIntentNodeAsync(TitleVersion titleVersion, int badIntentCount)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            var storyTitle = await GetByIdAsync(titleVersion);

            List<StoryNode> badIntentNodes = storyTitle.BadIntentResponses;

            if (badIntentNodes != null)
            {

                int badIntentIndex = badIntentCount - 1;

                if (badIntentNodes.Count > 0)
                {

                    if (badIntentNodes.Count < badIntentCount)
                    {
                        // Return the last one or apply a modulus.
                        return badIntentNodes[badIntentNodes.Count - 1];
                    }

                    return badIntentNodes[badIntentIndex];
                }
            }

            return null;
        }

        public async Task<List<StoryConditionBase>> GetStoryConditionsAsync(TitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));


            var storyTitle = await GetByIdAsync(titleVersion);

            return storyTitle?.Conditions;

        }

        public async Task<StoryType> GetStoryTypeAsync(TitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            var storyTitle = await GetByIdAsync(titleVersion);
            return storyTitle.StoryType;

        }

        public async Task<StoryNode> GetErrorNodeAsync(TitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            var storyTitle = await GetByIdAsync(titleVersion);

            List<StoryNode> errorNodes = storyTitle.ErrorResponses;

            if (errorNodes != null)
            {

                int errNodeIndex = StaticRandom.Next(0, errorNodes.Count - 1);

                StoryNode curNode = errorNodes[errNodeIndex];

                return curNode;

            }

            return null;
        }
    }
}
