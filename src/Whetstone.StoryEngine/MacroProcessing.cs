using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine
{
    public static class MacroProcessing
    {

        public static async Task<string> ProcessMacrosAsync(string unprocessedText)
        {
            string returnText = unprocessedText;

            try
            {
                if (!string.IsNullOrEmpty(unprocessedText))
                {

                    string codeExpression = @"@@(.*?)@@";
                    var matchCol = Regex.Matches(returnText, codeExpression);


                    foreach (Match mat in matchCol)
                    {
                        string codeText = mat.Value.Substring(2, mat.Value.Length - 4);

                        string evaluatedText = await ProcessMacroAsync(codeText);

                        returnText = returnText.Replace(mat.Value, evaluatedText);
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error replacing macro text {unprocessedText}", ex);
            }

            return returnText;
        }



        public static async Task<string> ProcessTextFragmentMacrosAsync(string responseText, List<SelectedItem> selectedItems, ILogger logger)
        {
            string returnText = responseText;
            string regExpression = @"(?<=\{)[^}]*(?=\})";


            try
            {
                if (!string.IsNullOrEmpty(responseText))
                {
                    Regex regex = new Regex(regExpression, RegexOptions.IgnoreCase);


                    MatchCollection matches = regex.Matches(responseText);


                    // Results include braces (undesirable)
                    var results = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();

                    foreach (string result in results)
                    {
                        if ((selectedItems?.Any()).GetValueOrDefault(false))
                        {
                            var selItem = selectedItems.FirstOrDefault(x =>
                                x.Name.Equals(result, StringComparison.OrdinalIgnoreCase));
                            if (selItem != null)
                            {
                                if (!string.IsNullOrWhiteSpace(selItem.Value))
                                {
                                    string replacementText = string.Concat("{", result, "}");

                                    returnText = returnText.Replace(replacementText, selItem.Value);
                                }
                            }
                        }
                        else
                        {
                            logger.LogError(
                                $"Replacement token {result} not found in selected item(s) for text {responseText}");
                        }
                    }



                    string codeExpression = @"@@(.*?)@@";
                    var matchCol = Regex.Matches(returnText, codeExpression);


                    foreach (Match mat in matchCol)
                    {
                        string codeText = mat.Value.Substring(2, mat.Value.Length - 4);

                        string evaluatedText = await ProcessMacroAsync(codeText);

                        returnText = returnText.Replace(mat.Value, evaluatedText);
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error replacing macro text {responseText} in ProcessTextFragmentMacrosAsync", ex);
            }

            return returnText;
        }

        private static async Task<string> ProcessMacroAsync(string codeText)
        {
            string evaluatedText;


            string processText = codeText.Trim();


            int openParensIndex = processText.IndexOf("(", StringComparison.Ordinal);

            string macroCommand = processText.Substring(0, openParensIndex);

            macroCommand = macroCommand.Trim();

            if (macroCommand.Equals("FormatPhoneNumber", StringComparison.OrdinalIgnoreCase))
            {
                //string argText = GetArg
                var argText = GetArgText(processText);


                evaluatedText = PhoneUtility.FormatPhoneNumberUS(argText);
            }
            else if (macroCommand.Equals("FormatPhoneNumberWithSpaces", StringComparison.OrdinalIgnoreCase))
            {
                var argText = GetArgText(processText);

                evaluatedText = PhoneUtility.FormatPhoneNumberWithSpacesUS(argText);
            }
            else if (macroCommand.Equals("FormatUTC", StringComparison.OrdinalIgnoreCase))
            {
                var argText = GetArgText(processText);

                evaluatedText = DateTime.UtcNow.ToString(argText);
            }
            else
            {
                codeText = string.Concat("using System; ", codeText);
                evaluatedText = await CSharpScript.EvaluateAsync<string>(codeText);
            }

            return evaluatedText;
        }

        private static string GetArgText(string processText)
        {
            int openParensIndex = processText.IndexOf("(", StringComparison.Ordinal);
            int closeParensIndex = processText.LastIndexOf(")", StringComparison.Ordinal);

            string argText = processText.Substring(openParensIndex + 1, (closeParensIndex - openParensIndex) - 1);

            argText = argText.Trim();

            // Get the text inside the quotation marks if there are quotes.
            if (argText[0].Equals('\"') && argText[argText.Length - 1].Equals('\"'))
            {
                argText = argText.Substring(1, argText.Length - 2);
                argText = argText.Trim();
            }



            return argText;
        }


        public static List<SelectedItem> GetSelectedItems(this List<IStoryCrumb> crumbs)
        {
            List<SelectedItem> selItems = new List<SelectedItem>();
            if ((crumbs?.Any()).GetValueOrDefault(false))
            {
                foreach (IStoryCrumb crumb in crumbs)
                {
                    if (crumb is SelectedItem)
                        selItems.Add((SelectedItem)crumb);
                }
            }

            return selItems;
        }

    }
}
