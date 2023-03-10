using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Whetstone.StoryEngine.Models.Actions;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public static class ActionExtensions
    {


        public static void AddActionProcessors(this IServiceCollection services)
        {

            services.AddTransient<AssignSlotValueActionProcessor>();
            services.AddTransient<ResetStateActionProcessor>();
            services.AddTransient<NodeVisitRecordActionProcessor>();
            services.AddTransient<RemoveSelectedItemActionProcessor>();
            services.AddTransient<RecordSelectedItemActionProcessor>();

            services.AddTransient<InventoryActionProcessor>();
            services.AddTransient<ResetStateActionProcessor>();
            services.AddTransient<GetPersonalInfoActionProcessor>();

            services.AddTransient<Func<NodeActionEnum, INodeActionProcessor>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case NodeActionEnum.AssignValue:
                        return serviceProvider.GetService<AssignSlotValueActionProcessor>();
                    case NodeActionEnum.GetPersonalDataAction:
                        return serviceProvider.GetService<GetPersonalInfoActionProcessor>();
                    case NodeActionEnum.Inventory:
                        return serviceProvider.GetService<InventoryActionProcessor>();
                    case NodeActionEnum.NodeVisit:
                        return serviceProvider.GetService<NodeVisitRecordActionProcessor>();
                    case NodeActionEnum.RemoveSelectedItem:
                        return serviceProvider.GetService<RemoveSelectedItemActionProcessor>();
                    case NodeActionEnum.ResetState:
                        return serviceProvider.GetService<ResetStateActionProcessor>();
                    case NodeActionEnum.SelectedItem:
                        return serviceProvider.GetService<RecordSelectedItemActionProcessor>();
                    default:
                        throw new KeyNotFoundException();
                }
            });

        }

    }
}
