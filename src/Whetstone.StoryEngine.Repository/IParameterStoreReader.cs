using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository
{
    /// <summary>
    /// Return a value from the parameter store.
    /// </summary>
    public interface IParameterStoreReader
    {

        /// <summary>
        /// Retrieves the contents of a parameter from 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns>Contents of the parameter</returns>
        Task<string> GetValueAsync(string parameterName);

    }
}
