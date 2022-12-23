using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{
    public interface IStoryDataItem
    {
        long? Id { get; set; }

        Guid UniqueId { get; set; }
    }
}
