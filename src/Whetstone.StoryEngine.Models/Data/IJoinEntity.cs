using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{
    public interface IJoinEntity<TEntity>
    {
        TEntity Navigation { get; set; }
    }
}
