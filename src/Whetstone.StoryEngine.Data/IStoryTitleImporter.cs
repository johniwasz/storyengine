using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Data
{
    public interface IStoryTitleImporter
    {


        Task ImportFromZipAsync(byte[] importZip);
            
    }
}
