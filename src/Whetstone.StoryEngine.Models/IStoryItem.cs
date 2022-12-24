using System;

namespace Whetstone.StoryEngine.Models
{
    public interface IStoryItem
    {

        long? Id { get; set; }

        Guid? UniqueId { get; set; }
    }
}
