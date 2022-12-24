using System.Text.RegularExpressions;
using Xunit;

namespace Whetstone.StoryEngine.Test
{
    public class RegExValidator
    {

        [Fact]
        public void GuidRegExTester()
        {

            string subjectString = "92304d4d-42a5-4371-9b13-97b4a79b9ad0";


            var resultString = Regex.Replace(subjectString,
                @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$",
                "'$0'");

        }
    }
}
