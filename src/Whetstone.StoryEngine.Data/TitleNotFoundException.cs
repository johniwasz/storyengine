using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public class TitleNotFoundException : Exception
    {

        public TitleNotFoundException(string message) : base(message)
        {


        }

        public TitleNotFoundException(string message, Exception innerEx) : base(message, innerEx)
        {


        }


      

    }
}
