using System;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class DuplicateKeyException : Exception
    {


        public DuplicateKeyException()
        {

        }

        public DuplicateKeyException(string message) : base(message)
        {

        }

        public DuplicateKeyException(string message, Exception innerEx) : base(message, innerEx)
        {

        }


        public DuplicateKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {


        }

    }
}
