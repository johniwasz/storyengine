using System.IO;
using System.Reflection;
using System.Text;

namespace Whetstone.UnitTests
{
    internal class ResouceFileUtil
    {

        internal static string GetJsonContents(string fileName)
        {



            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"Whetstone.StoryEngine.UnitTests.{fileName}.json";
            string retContent = null;

            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                retContent = reader.ReadToEnd();
            }

            return retContent;
        }

    }
}
