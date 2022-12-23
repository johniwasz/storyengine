using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.Test.Google
{

    [DebuggerDisplay("Millisecods: {Milliseconds}")]
    public class TimingResponse
    {
        public long Milliseconds { get; set; }


        public Exception Ex { get; set; }

    }
}
