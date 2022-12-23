using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;

namespace Whetstone.StoryEngine.Data.Create.Migrations
{
    [ExcludeFromCodeCoverage]
    internal static class MigrationFunctionExtensions
    {

        internal static string GetFunctionContent(string functionName)
        {
            string resourceName = $"Whetstone.StoryEngine.Data.Create.Migrations.Functions.{functionName}.sql";


            return GetEmbeddedContent(resourceName);
        }

        internal static string GetScriptContent(string scriptName)
        {
            string resourceName = $"Whetstone.StoryEngine.Data.Create.Migrations.Scripts.{scriptName}.sql";


            return GetEmbeddedContent(resourceName);
        }

        private static string GetEmbeddedContent(string resourceName)
        {
            string retContent = null;

            try
            {

                var assembly = Assembly.GetExecutingAssembly();


                var resourceStream = assembly.GetManifestResourceStream(resourceName);
                using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
                {
                    retContent = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                
                throw new Exception($"Error getting resource {resourceName}", ex);
            }



            return retContent;


        }

    }
}
