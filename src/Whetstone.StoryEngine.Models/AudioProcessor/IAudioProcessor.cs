namespace Whetstone.StoryEngine.Models.AudioProcessor
{
    public interface IAudioProcessor
    {
        IAudioFileInfo GetAudioFileInfo(string inputFie);
        void ConvertAudioFile(string inputFile, string outputFile);
    }
}
