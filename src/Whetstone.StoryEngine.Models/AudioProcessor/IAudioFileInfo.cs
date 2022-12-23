using System;

namespace Whetstone.StoryEngine.Models.AudioProcessor
{
    public interface IAudioFileInfo
    {
        bool IsAudioFile { get; }
        bool IsValidAudioFile { get; }
        string AudioType { get;  }
        int BitRate { get; }
        int SampleRate { get; }

    }
}
