using System.Text.RegularExpressions;

using Whetstone.StoryEngine.Models.AudioProcessor;

namespace Whetstone.StoryEngine.AudioProcessor.Repository
{
    public class FFProbeAudioFileInfo : IAudioFileInfo
    {
        private const string DURATION = "Duration: ";
        private const string BITRATE = "bitrate: ";
        private const string AUDIO = "Audio: ";

        private const int REQUIRED_SAMPLERATE = 24000;
        private const int REQUIRED_BITRATE = 48; // in kb/s

        public readonly bool _isAudioFile = false;
        public readonly string _audioType;
        public readonly int _bitRate;
        public readonly int _sampleRate;

        public FFProbeAudioFileInfo(string ffprobeOutput)
        {

            // Looks for some specific cues in the ffprobeOutput text to tell us if
            // we're dealing with an audio file and if we are, then what type it is.

            int durationIndex = ffprobeOutput.IndexOf(DURATION);

            if (durationIndex != -1)
            {
                string audioInfo = ffprobeOutput.Substring(durationIndex);

                string[] lines = audioInfo.Split('\n');

                if (lines.Length == 3)
                {
                    int audioIndex = lines[1].IndexOf(AUDIO);

                    if (audioIndex != -1)
                    {
                        string bitrate = lines[0].Substring(lines[0].IndexOf(BITRATE) + BITRATE.Length);

                        this._bitRate = ExtractInteger(bitrate);

                        string audioDetails = lines[1].Substring(audioIndex + AUDIO.Length);

                        lines = audioDetails.Split(", ");
                        this._audioType = lines[0];

                        this._sampleRate = ExtractInteger(lines[1]);

                        this._isAudioFile = true;
                    }
                }
            }

        }

        public bool IsAudioFile { get { return this._isAudioFile; } }
        public string AudioType { get { return this._audioType; } }
        public int BitRate { get { return this._bitRate; } }
        public int SampleRate { get { return this._sampleRate; } }

        public bool IsValidAudioFile
        {
            get
            {
                return this._audioType.StartsWith("mp3") && this._bitRate == REQUIRED_BITRATE && this._sampleRate == REQUIRED_SAMPLERATE;
            }
        }

        private static int ExtractInteger(string s)
        {
            Regex re = new Regex(@"\d+");
            Match m = re.Match(s);

            return int.Parse(m.Value);
        }
    }
}
