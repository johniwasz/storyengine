using System;

namespace Whetstone.StoryEngine.Models.Data
{
    public interface IStoryDataItem
    {
        long? Id { get; set; }

        Guid UniqueId { get; set; }
    }
}
