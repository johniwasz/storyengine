using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Serilog;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.UnitTests
{
    public class AssignItemTest
    {
        [Fact]
        public async Task ApplyAssignItemAsync()
        {

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
            .WriteTo.Console()
             .WriteTo.Debug()
             
            .CreateLogger();

            ILoggerFactory logger = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(loggerConfig, dispose: false);
                
             
              
            });


           ILogger<AssignSlotValueActionProcessor> valueLogger = logger.CreateLogger<AssignSlotValueActionProcessor>();

            Mock<ITitleReader> titleMock = new Mock<ITitleReader>();

            titleMock.Setup(x => x.IsPrivacyLoggingEnabledAsync(It.IsAny<TitleVersion>())).Returns(() => 
            
            
            Task.Run(() => true));


            AssignSlotValueActionProcessor assignSlotValues = new AssignSlotValueActionProcessor(valueLogger, titleMock.Object);

            StoryRequest req = new StoryRequest();

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();

            SelectedItem selItem = new SelectedItem();
            selItem.Name = "letter";
            selItem.Value = "JOHN";
            crumbs.Add(selItem);

            AssignSlotValueActionData assignSlotData = new AssignSlotValueActionData();
            assignSlotData.SlotName = "firstname";
            assignSlotData.Value = "{letter}";

            req.SessionContext = new EngineSessionContext();
            req.SessionContext.TitleVersion = new TitleVersion();

            string actionLog = await assignSlotValues.ApplyActionAsync(req, crumbs, assignSlotData);





        }


    }
}
