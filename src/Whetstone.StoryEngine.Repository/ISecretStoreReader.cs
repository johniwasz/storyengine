using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository
{
    public interface ISecretStoreReader
    {


        /// <summary>
        /// Retrieves the contents of a parameter from 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns>Contents of the parameter</returns>
        Task<string> GetValueAsync(string parameterName);

    }
}
