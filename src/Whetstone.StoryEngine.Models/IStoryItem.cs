using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models
{
    public interface IStoryItem
    {

        long? Id { get; set; }

        Guid? UniqueId { get; set; }
    }
}
