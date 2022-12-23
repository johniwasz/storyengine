using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public interface IJoinEntity<TEntity>
    {
        TEntity Navigation { get; set; }
    }
}
