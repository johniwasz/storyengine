using Whetstone.StoryEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models
{
    public interface INodeProcessor
    {

        StoryResponse ProcessNode();

    }
}
