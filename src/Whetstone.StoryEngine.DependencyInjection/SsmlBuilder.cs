using System.Collections.Generic;
using System.Linq;
using System.Text;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.DependencyInjection
{
    public static class SsmlBuilder
    {

        public static string ToSsml(this IEnumerable<SpeechFragment> fragments,  IMediaLinker mediaLinker, TitleVersion titleVer)
        {


            StringBuilder ssmlBuilder = new StringBuilder();


            ssmlBuilder.Append("<speak>");
            ProcessFragments(fragments, ssmlBuilder, mediaLinker, titleVer);
            ssmlBuilder.Append("</speak>");

            string outtext = ssmlBuilder.ToString();


            return outtext;



        }

        private static void ProcessFragments(IEnumerable<SpeechFragment> fragments, StringBuilder ssmlBuilder, IMediaLinker mediaLinker, TitleVersion titleVer)
        {


            foreach (SpeechFragment fragment in fragments)
            {

                if (fragment is PlainTextSpeechFragment)
                {
                    PlainTextSpeechFragment fragmentFrag = (PlainTextSpeechFragment)fragment;
                    ssmlBuilder.Append(fragmentFrag.Text);
                    // Include a space to separate the fragments.
                    ssmlBuilder.Append(" ");
                }

                if (fragment is DirectAudioFile)
                {
                    DirectAudioFile directAudio = (DirectAudioFile)fragment;
                    ssmlBuilder.AppendFormat("<audio src='{0}'/>", directAudio.AudioUrl.ToString());
                }


                if (fragment is AudioFile)
                {
                    AudioFile audioFile = (AudioFile)fragment;
                    ssmlBuilder.AppendFormat("<audio src='{0}'/>", mediaLinker.GetFileLink(titleVer, audioFile.FileName));
                }

                if (fragment is SsmlSpeechFragment)
                {
                    // TODO if a voice is specified, the include that audio source
                    SsmlSpeechFragment ssmlFrag = (SsmlSpeechFragment)fragment;
                    ssmlBuilder.Append(ssmlFrag.Ssml);
                }

                if (fragment is SpeechBreakFragment)
                {
                    SpeechBreakFragment breakFrag = (SpeechBreakFragment)fragment;
                    ssmlBuilder.AppendFormat("<break time='{0}ms'/>", breakFrag.Duration);

                }
            }

        }

        public static ClientSpeechFragments GetClientSpeechFragments(this LocalizedResponse localizedResponse, Client speechClient)
        {
            var speechResponse = localizedResponse?.SpeechResponses?.FirstOrDefault(x => x.SpeechClient == speechClient);

            if (speechResponse == null)
            {
                speechResponse = localizedResponse?.SpeechResponses?.FirstOrDefault(x => x.SpeechClient == null);
            }

            return speechResponse;
        }

    }
}
