namespace Whetstone.StoryEngine.Data
{
    public interface IStoryTitleSerializer
    {
        void SaveToYamlFile(string title, string fileName, bool overwriteFile = false);

        void SaveToZip(string title, string fileName, bool overwriteFile = false);
    }
}
