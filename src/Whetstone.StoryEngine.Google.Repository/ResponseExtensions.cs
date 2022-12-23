using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Google.Repository
{
    internal static class ResponseExtensions
    {


        internal static string GetCleanText(this List<TextFragmentBase> baseFragments)
        {
            List<string> simpleTextFrags = new List<string>();
            foreach (TextFragmentBase frag in baseFragments)
            {
                if (frag is SimpleTextFragment simpleFrag)
                {
                    string cleanString = simpleFrag.Text;
                    cleanString = String.IsNullOrWhiteSpace(cleanString) ? null : cleanString.Replace("’", @"'")
                        .Replace("“", "\"")
                        .Replace("”", "\"")
                        .Replace("–", "-");
                    if (!string.IsNullOrWhiteSpace(cleanString))
                    {
                        simpleTextFrags.Add(cleanString);
                    }
                }


            }

            return simpleTextFrags.JoinList();
        }
    }
}
