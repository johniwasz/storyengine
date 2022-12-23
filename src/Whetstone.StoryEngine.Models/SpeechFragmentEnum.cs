using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models
{
    public enum SpeechFragmentType
    {
        AudioFile = 1,
        DirectAudioFile =2,
        PlainText = 3,
        ConditionalFragment = 4,
        Ssml = 5,
        Break = 6,
        SwitchFragment = 7
    }
}
