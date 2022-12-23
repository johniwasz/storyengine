using Whetstone.StoryEngine.Models.Story.Ssml;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Serialization
{
    public class YamlSpeechFragmentResolver : ITypeResolver
    {



        public Type Resolve(Type staticType, object actualValue)
        {
            DirectAudioFile audioFile = actualValue as DirectAudioFile;

            if (audioFile != null)
                return typeof(DirectAudioFile);


            PlainTextSpeechFragment fragment = actualValue as PlainTextSpeechFragment;

            if (fragment != null)
                return typeof(PlainTextSpeechFragment);


            AudioFile audio = actualValue as AudioFile;
            if (audio != null)
                return typeof(AudioFile);

            return null;
        }

    }
}
