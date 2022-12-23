using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public interface IEntityBase<TID>
    {
         TID Id { get; set; }


    }
}
