using System;

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
