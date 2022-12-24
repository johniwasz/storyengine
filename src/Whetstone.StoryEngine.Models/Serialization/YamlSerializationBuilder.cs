using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Integration;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Models.Tracking;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public static class YamlSerializationBuilder
    {


        public static ISerializer GetYamlSerializer()
        {


            var yamlSerializer = new SerializerBuilder()
               .DisableAliases()
                 .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
             .WithTypeConverter(new YamlUriTypeConverter())
             .WithTypeConverter(new YamlStringEnumConverter())
             .WithTagMapping("!dr-tablesearch", typeof(TableFunctionSearchAction))
              .WithTagMapping("!dr-externalfunction", typeof(ExternalFunctionAction))
             .WithTagMapping("!sf-break", typeof(SpeechBreakFragment))
             .WithTagMapping("!sf-ssmlfrag", typeof(SsmlSpeechFragment))
             .WithTagMapping("!sf-textfrag", typeof(PlainTextSpeechFragment))
             .WithTagMapping("!sf-directaudio", typeof(DirectAudioFile))
             .WithTagMapping("!sf-audio", typeof(AudioFile))
             .WithTagMapping("!sf-condition", typeof(ConditionalFragment))
            .WithTagMapping("!sf-switch", typeof(SwitchConditionFragment))
             .WithTagMapping("!tf-simple", typeof(SimpleTextFragment))
              .WithTagMapping("!tf-condition", typeof(ConditionalTextFragment))
               .WithTagMapping("!tf-audio", typeof(AudioTextFragment))
             .WithTagMapping("!na-inventory", typeof(InventoryActionData))
             .WithTagMapping("!na-recordvisit", typeof(NodeVisitRecordActionData))
             .WithTagMapping("!na-recordselecteditem", typeof(RecordSelectedItemActionData))
             .WithTagMapping("!na-removeselecteditem", typeof(RemoveSelectedItemActionData))
             .WithTagMapping("!na-phonemessage", typeof(PhoneMessageActionData))
             .WithTagMapping("!na-resetstate", typeof(ResetStateActionData))
             .WithTagMapping("!na-assignvalue", typeof(AssignSlotValueActionData))
             .WithTagMapping("!na-getpersonaldata", typeof(GetPersonalInfoActionData))
            .WithTagMapping("!na-validatephone", typeof(ValidatePhoneNumberActionData))
            .WithTagMapping("!na-setsmsconfirmation", typeof(SmsConfirmationActionData))
             .WithTagMapping("!nt-uniqueitem", typeof(UniqueItem))
             .WithTagMapping("!nt-multiitem", typeof(MultiItem))
             .WithTagMapping("!ci-inventory", typeof(InventoryCondition))
             .WithTagMapping("!ci-nodevisit", typeof(NodeVisitCondition))
            .WithTagMapping("!ci-clienttype", typeof(UserClientCondition))
            .WithTagMapping("!ci-slotvalue", typeof(SlotValueCondition))
            .WithTagMapping("!nm-singlenode", typeof(SingleNodeMapping))
            .WithTagMapping("!nm-multinode", typeof(MultiNodeMapping))
            .WithTagMapping("!nm-conditional", typeof(ConditionalNodeMapping))
            .WithTagMapping("!nm-slotmap", typeof(SlotMap))
            .WithTagMapping("!bt-link", typeof(LinkButton))
             .Build();




            return yamlSerializer;
        }


        public static IDeserializer GetYamlDeserializer()
        {
            var yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(new YamlUriTypeConverter())
            .WithTypeConverter(new YamlStringEnumConverter())
             .WithTagMapping("!dr-tablesearch", typeof(TableFunctionSearchAction))
              .WithTagMapping("!dr-externalfunction", typeof(ExternalFunctionAction))
                .WithTagMapping("!sf-break", typeof(SpeechBreakFragment))
                .WithTagMapping("!sf-ssmlfrag", typeof(SsmlSpeechFragment))
            //  .WithTypeConverter(new YamlLocalizedResponseSetTypeConverter())
            .WithTagMapping("!sf-textfrag", typeof(PlainTextSpeechFragment))
            .WithTagMapping("!sf-directaudio", typeof(DirectAudioFile))
            .WithTagMapping("!sf-audio", typeof(AudioFile))
            .WithTagMapping("!sf-condition", typeof(ConditionalFragment))
            .WithTagMapping("!sf-switch", typeof(SwitchConditionFragment))

            .WithTagMapping("!tf-simple", typeof(SimpleTextFragment))
            .WithTagMapping("!tf-condition", typeof(ConditionalTextFragment))
            .WithTagMapping("!tf-audio", typeof(AudioTextFragment))
            .WithTagMapping("!na-inventory", typeof(InventoryActionData))
            .WithTagMapping("!na-recordvisit", typeof(NodeVisitRecordActionData))
            .WithTagMapping("!na-recordselecteditem", typeof(RecordSelectedItemActionData))
            .WithTagMapping("!na-removeselecteditem", typeof(RemoveSelectedItemActionData))
            .WithTagMapping("!na-phonemessage", typeof(PhoneMessageActionData))
            .WithTagMapping("!na-resetstate", typeof(ResetStateActionData))
            .WithTagMapping("!na-assignvalue", typeof(AssignSlotValueActionData))
            .WithTagMapping("!na-getpersonaldata", typeof(GetPersonalInfoActionData))
            .WithTagMapping("!na-validatephone", typeof(ValidatePhoneNumberActionData))
            .WithTagMapping("!na-setsmsconfirmation", typeof(SmsConfirmationActionData))
            .WithTagMapping("!nt-uniqueitem", typeof(UniqueItem))
            .WithTagMapping("!nt-multiitem", typeof(MultiItem))
            .WithTagMapping("!ci-inventory", typeof(InventoryCondition))
            .WithTagMapping("!ci-nodevisit", typeof(NodeVisitCondition))
            .WithTagMapping("!ci-clienttype", typeof(UserClientCondition))
            .WithTagMapping("!ci-slotvalue", typeof(SlotValueCondition))
            .WithTagMapping("!nm-singlenode", typeof(SingleNodeMapping))
            .WithTagMapping("!nm-multinode", typeof(MultiNodeMapping))
            .WithTagMapping("!nm-conditional", typeof(ConditionalNodeMapping))
            .WithTagMapping("!nm-slotmap", typeof(SlotMap))
            .WithTagMapping("!bt-link", typeof(LinkButton))
            //.WithObjectFactory(objectFactory)
            .Build();

            return yamlDeserializer;
        }
    }
}
