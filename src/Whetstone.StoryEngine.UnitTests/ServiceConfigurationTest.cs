using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Data;

namespace Whetstone.UnitTests
{
    public class ServiceConfigurationTest
    {

        [Fact]
        public void ConfigureActionServices()
        {

            MockFactory mocker = new MockFactory();

            IServiceCollection services = mocker.InitServiceCollection("sometitle");

            //ITitleReader reader = MockFactory.GetTitleReader();

            //services.AddTransient<ITitleReader>(x => reader);

            //services.AddActionProcessors();           

            var provider = services.BuildServiceProvider();


            Func<NodeActionEnum, INodeActionProcessor> actionFunc = provider.GetService<Func<NodeActionEnum, INodeActionProcessor>>();

           InventoryActionProcessor invProcessor = (InventoryActionProcessor) actionFunc(NodeActionEnum.Inventory);
            Assert.NotNull(invProcessor);

            AssignSlotValueActionProcessor slotValueProcessor = (AssignSlotValueActionProcessor)actionFunc(NodeActionEnum.AssignValue);
            Assert.NotNull(slotValueProcessor);


            GetPersonalInfoActionProcessor personalInfoProcessor = (GetPersonalInfoActionProcessor)actionFunc(NodeActionEnum.GetPersonalDataAction);
            Assert.NotNull(personalInfoProcessor);


            NodeVisitRecordActionProcessor nodeVisitProcessor = (NodeVisitRecordActionProcessor)actionFunc(NodeActionEnum.NodeVisit);
            Assert.NotNull(nodeVisitProcessor);

        
            PhoneMessageActionProcessor phoneProcessor = (PhoneMessageActionProcessor)actionFunc(NodeActionEnum.PhoneMessage);
            Assert.NotNull(phoneProcessor);


            ResetStateActionProcessor resetProcessor = (ResetStateActionProcessor)actionFunc(NodeActionEnum.ResetState);
            Assert.NotNull(resetProcessor);


            RemoveSelectedItemActionProcessor removeItemProcessor = (RemoveSelectedItemActionProcessor)actionFunc(NodeActionEnum.RemoveSelectedItem);
            Assert.NotNull(removeItemProcessor);

            RecordSelectedItemActionProcessor recordItemProcessor = (RecordSelectedItemActionProcessor)actionFunc(NodeActionEnum.SelectedItem);
            Assert.NotNull(recordItemProcessor);



           // provider.GetService<INodeActionProcessor>()

        }

      
    }
}
