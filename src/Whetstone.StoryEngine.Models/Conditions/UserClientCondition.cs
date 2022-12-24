using MessagePack;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Conditions
{
    public class UserClientCondition : StoryConditionBase
    {

        public override bool IsStoryCondition(ConditionInfo conditionInfo)
        {
            if (conditionInfo == null)
                throw new ArgumentNullException(nameof(conditionInfo));

            return conditionInfo.UserClient == this.ClientType;
        }


        public UserClientCondition()
        {
            this.ConditionType = ConditionType.UserClientCondition;
        }


        public UserClientCondition(string name)
        {
            this.ConditionType = ConditionType.UserClientCondition;
            this.Name = name;

        }

        [YamlIgnore]
        [DataMember]
        [Key(1)]
        [NotMapped]
        [JsonProperty("conditionType", Order = 0)]
        public sealed override ConditionType ConditionType { get; set; }

        [DataMember]
        [Key(2)]
        [JsonProperty("clientType", Order = 1)]
        public Client ClientType { get; set; }
    }
}
