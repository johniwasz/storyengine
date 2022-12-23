using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Models.Tracking;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.StoryEngine.Repository.Phone;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.UnitTests
{
    public class PhoneTests
    {
        [Fact]
        public async Task PhoneActionTestAsync()
        {

            var validateProcessor = GetPhoneValidator();

            StoryRequest storyReq = new StoryRequest();
            storyReq.SessionContext = new EngineSessionContext();
            storyReq.SessionContext.TitleVersion = new TitleVersion();

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();
            SelectedItem selItem = new SelectedItem();
            selItem.Name = "phonenumber";
            selItem.Value = "2673401278";
            crumbs.Add(selItem);

            ValidatePhoneNumberActionData valActionData = new ValidatePhoneNumberActionData();
            valActionData.NodeAction = NodeActionEnum.ValidatePhoneNumber;
            valActionData.IsValidFormatSlot = "isvalidformat";
            valActionData.PhoneNumberSlot = "phonenumber";


            string actionResultText = await validateProcessor.ApplyActionAsync(storyReq, crumbs, valActionData);


            var selItems = crumbs.GetSelectedItems();

            Assert.True(selItems.Count == 2);

           SelectedItem isValidItem  = selItems.FirstOrDefault(x => x.Name.Equals("isvalidformat"));

            Assert.NotNull(isValidItem);

            Assert.Equal("true", isValidItem.Value, ignoreCase: true);

        }



        [Theory]       
        [InlineData("2675551212", "false", "Other")]
        [InlineData("123", "false", "InvalidFormat")]
        [InlineData("1234567", "false", "InvalidFormat")]
        public async Task PhoneActionTypeTestAsync(string phoneNumber, string supportsSmsExpected, string expectedPhoneType)
        {

            var validateProcessor = GetPhoneValidator();

            StoryRequest storyReq = new StoryRequest();
            storyReq.SessionContext = new EngineSessionContext();
            storyReq.SessionContext.TitleVersion = new TitleVersion();

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();
            SelectedItem selItem = new SelectedItem();
            selItem.Name = "phonenumber";
            selItem.Value = phoneNumber;
            crumbs.Add(selItem);

            ValidatePhoneNumberActionData valActionData = new ValidatePhoneNumberActionData();
            valActionData.NodeAction = NodeActionEnum.ValidatePhoneNumber;
            valActionData.SupportsSmsSlot = "supportsms";
            valActionData.PhoneTypeSlot = "phonetype";


            valActionData.PhoneNumberSlot = "phonenumber";


            string actionResultText = await validateProcessor.ApplyActionAsync(storyReq, crumbs, valActionData);


            var selItems = crumbs.GetSelectedItems();

            Assert.True(selItems.Count == 3);

            SelectedItem supportsSms = selItems.FirstOrDefault(x => x.Name.Equals("supportsms"));

            Assert.NotNull(supportsSms);

            Assert.Equal(supportsSms.Value, supportsSmsExpected, ignoreCase: true);

            SelectedItem phoneType = selItems.FirstOrDefault(x => x.Name.Equals("phonetype"));

            Assert.NotNull(phoneType);

            Assert.Equal(phoneType.Value, expectedPhoneType, ignoreCase: true);


        }

        private static ValidatePhoneNumberActionProcessor GetPhoneValidator()
        {
            MockFactory mockFac = new MockFactory();

            var whetstoneVer = TitleVersionUtil.GetWhetstoneTitle();

            IServiceCollection servCol = mockFac.InitServiceCollection(whetstoneVer);
            


            IServiceProvider servProv = servCol.BuildServiceProvider();

            IPhoneInfoRetriever phoneRetriever = servProv.GetRequiredService<IPhoneInfoRetriever>();

            ITitleReader titleRedeader = servProv.GetRequiredService<ITitleReader>();


            ValidatePhoneNumberActionProcessor validateProcessor = new ValidatePhoneNumberActionProcessor(phoneRetriever, titleRedeader);
            return validateProcessor;
        }


        [Theory(DisplayName = "Phone Number Validation Tests")]
        [InlineData("98005551212", false)]
        [InlineData("2675551212", true)]
        [InlineData("18005551212", true)]
        [InlineData("1234567", false)]
        [InlineData("It's 160 yes", false)]
        [InlineData("?", false)]
        [InlineData("11111111111", false)]
        [InlineData("111111111111", false)]
        [InlineData("1234", false)]
        public void PhoneValidationTest(string number, bool expectedValidResult)
        {
            Stopwatch numberCheckTime = new Stopwatch();
            numberCheckTime.Start();
            var formattedNumber = PhoneUtility.ValidateFormat(number, "en-us");
            numberCheckTime.Stop();
            Debug.WriteLine(numberCheckTime.ElapsedMilliseconds);

            Assert.True(expectedValidResult == formattedNumber.IsValid,
                $"Supplied number {number} and expected valid result {expectedValidResult} and got {formattedNumber.IsValid} with formatting {formattedNumber.FormattedNumber}");

        }


        [Theory(DisplayName = "Phone Number Validation Tests")]
        [InlineData("18005551212", "1-800-555-1212")]
        [InlineData("2675551212", "267-555-1212")]
        [InlineData("98005551212", null)]
        [InlineData("1234567", null)]
        [InlineData("267.304.1278", "267-555-1212")]
        [InlineData("It's 160 yes", null)]
        [InlineData("?", null)]
        [InlineData("11111111111", null)]
        [InlineData("111111111111", null)]
        [InlineData("1234", null)]
        [InlineData(null, null)]
        [InlineData("", null)]
        public void PhoneFormatTest(string number, string expectedResult)
        {
            var validateFormatResult = PhoneUtility.ValidateFormat(number, "en-US");
            string formattedPhoneNumber = null;
            if (validateFormatResult.IsValid)
            {
                formattedPhoneNumber = PhoneUtility.FormatPhoneNumberUS(number);
            }

            if (expectedResult == null)
            {
                Assert.Null(formattedPhoneNumber);
            }
            else if(formattedPhoneNumber ==null)
            {
                Assert.True(false, $"Expected {expectedResult} from number {number} and got a null result");
            }
            else
            {
                Assert.True(formattedPhoneNumber.Equals(expectedResult), $"Expected result {expectedResult} from number {number} and received {formattedPhoneNumber}");
            }
        }

        [Theory(DisplayName = "Phone Number With Spaces Validation Tests")]
        [InlineData("18005551212", "1 8 0 0 5 5 5 1 2 1 2")]
        [InlineData("2675551212", "2 6 7 3 0 4 1 2 7 8")]
        [InlineData("98005551212", null)]
        [InlineData("1234567", null)]
        [InlineData("267.304.1278", "2 6 7 3 0 4 1 2 7 8")]
        [InlineData(null, null)]
        [InlineData("", null)]
        public void PhoneFormatWithSpacesTest(string number, string expectedResult)
        {
            var validateFormatResult = PhoneUtility.ValidateFormat(number, "en-US");
            string formattedPhoneNumber = null;
            if (validateFormatResult.IsValid)
            {
                formattedPhoneNumber = PhoneUtility.FormatPhoneNumberWithSpacesUS(number);
            }

            if (expectedResult == null)
            {
                Assert.Null(formattedPhoneNumber);
            }
            else if (formattedPhoneNumber == null)
            {
                Assert.True(false, $"Expected {expectedResult} from number {number} and got a null result");
            }
            else
            {
                Assert.True(formattedPhoneNumber.Equals(expectedResult), $"Expected result {expectedResult} from number {number} and received {formattedPhoneNumber}");
            }
        }

        [Theory(DisplayName = "Phone Number Macro Validation Tests")]
        [InlineData("The number I heard you say, <say-as interpret-as=\"telephone\">@@FormatPhoneNumber(\"{phonenumber}\")@@</say-as>, is not a valid phone number.",
            "2673331212",
            "The number I heard you say, <say-as interpret-as=\"telephone\">267-333-1212</say-as>, is not a valid phone number.")]
        [InlineData("The number I heard you say, <say-as interpret-as=\"telephone\">@@String.Format(\"{0:###-###-####}\", {phonenumber})@@</say-as>, is not a valid phone number.",
            "2673331212",
            "The number I heard you say, <say-as interpret-as=\"telephone\">267-333-1212</say-as>, is not a valid phone number.")]
        [InlineData("The number I heard you say, @@FormatPhoneNumberWithSpaces(\"{phonenumber}\")@@, is not a valid phone number.",
            "2673331212",
            "The number I heard you say, 2 6 7 3 3 3 1 2 1 2, is not a valid phone number.")]
        public async Task ProcessPhoneMacroTestAsync(string rawText, string phoneNumber, string expectedResult)
        {
            ILogger logger = Mock.Of<ILogger>();

            List<SelectedItem> selItems = new List<SelectedItem>();

            SelectedItem phoneItem = new SelectedItem();
            phoneItem.Name = "phonenumber";
            phoneItem.Value = phoneNumber;
            selItems.Add(phoneItem);

            string processedText = await MacroProcessing.ProcessTextFragmentMacrosAsync(rawText, selItems, logger);

            Assert.True(processedText.Equals(expectedResult, StringComparison.InvariantCulture));

        }

    }
}
