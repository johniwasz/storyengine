using System;
using System.Diagnostics;

namespace Whetstone.StoryEngine.Test.Google
{

    [DebuggerDisplay("Millisecods: {Milliseconds}")]
    public class TimingResponse
    {
        public long Milliseconds { get; set; }


        public Exception Ex { get; set; }

    }
}
