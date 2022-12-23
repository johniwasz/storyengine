using System;
using System.Diagnostics.CodeAnalysis;

namespace Whetstone.StoryEngine.Data.Create
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            //StoryEngineContextFactory contextFactory = new StoryEngineContextFactory();

            //contextFactory.CreateDbContext(null);


            UserDataContextFactory userContextFactory = new UserDataContextFactory();

            userContextFactory.CreateDbContext(null);

        }
    }
}
