using CsvHelper;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Configuration;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class MessageConsentReportTest : DataTestBase
    {
        [Fact]
        public async Task MessageConsentReportTestAsync()
        {

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);

            //  var userContext = await userContextRetriever.GetContextOptionsAsync();

            MessageConsentReportRequest repRequest = new MessageConsentReportRequest();
            repRequest.TitleId = Guid.Parse("616a5f8e-fa08-41bf-b5a0-7a1ed4160db4");

            repRequest.StartTime = new DateTime(2019, 8, 10);
            repRequest.EndTime = new DateTime(2019, 8, 24);

            try
            {

                using (var context = await userContextRetriever.GetUserDataContextAsync())
                {
                    var messageList = await context.GetMessageConsentReportAsync(repRequest);


                    if (messageList != null)
                    {
                        StringBuilder csvTextBuilder = new StringBuilder();
                        using (var writer = new StringWriter(csvTextBuilder))
                        using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
                        {
                            csv.WriteRecords(messageList);
                        }

                        string csvText = csvTextBuilder.ToString();
                    }


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }



        }

    }
}
