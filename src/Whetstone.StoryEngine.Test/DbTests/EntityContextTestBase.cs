using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Test.DbTests
{

    public class EntityContextTestBase
    {

        protected DbContextOptions<T> GetContextOptions<T>(string connection) where T : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            optionsBuilder.EnableSensitiveDataLogging(true);

            optionsBuilder.UseNpgsql(connection);

            return optionsBuilder.Options;

        }


        protected ILogger<T> CreateLogger<T>()
        {

            ILoggerFactory factory = LoggerFactory.Create(
                builder =>
                {
                    builder.AddDebug();


                }

               );



            return factory.CreateLogger<T>();
        }


    }
}
