using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Whetstone.StoryEngine.Models.Tracking;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Conditions
{

    
    [DataContract]
    public class NodeVisitCondition : StoryConditionBase
    {

        public NodeVisitCondition()
        {
            this.ConditionType = ConditionType.NodeVisit;

        }


        [DataMember]
        [YamlMember]
        [JsonProperty("requiredNodes", Order = 1)]
        [Column(Order = 3)]
        public List<string> RequiredNodes { get; set; }

        [NotMapped]
        [YamlMember]
        [JsonProperty("conditionType", Order =0)]
        [DataMember]
        public override ConditionType ConditionType { get; set; }


        
        public override bool IsStoryCondition(ConditionInfo conditionInfo)
        {

            if (conditionInfo == null)
                throw new ArgumentNullException($"{nameof(conditionInfo)}");
            var storyCrumbs = conditionInfo.Crumbs;

            bool areNodesVisited = AreNodesVisited(storyCrumbs);

            // Check the permanent nodes.
            if(!areNodesVisited)
                areNodesVisited = AreNodesVisited(conditionInfo.PermanentCrumbs);


            if (RequiredOutcome.GetValueOrDefault(true))
                return areNodesVisited;
            

            return !areNodesVisited;
    
        }

        private bool AreNodesVisited(List<IStoryCrumb> storyCrumbs)
        {
            int foundRequiredNodeCount = 0;

            int requiredNodeCount = RequiredNodes.Count;

            if ((storyCrumbs?.Any()).GetValueOrDefault(false))
            {
                foreach (string requiredNode in RequiredNodes)
                {
                    NodeVisitRecord nodeRec = storyCrumbs.FirstOrDefault(x =>
                    {
                        if (x is NodeVisitRecord)
                        {
                            NodeVisitRecord testRec = x as NodeVisitRecord;

                            return testRec.Name.Equals(requiredNode, StringComparison.OrdinalIgnoreCase);
                        }

                        return false;
                    }) as NodeVisitRecord;


                    if (nodeRec != null)
                        foundRequiredNodeCount++;
                }
            }

            return foundRequiredNodeCount == requiredNodeCount;
        }
    }
}
