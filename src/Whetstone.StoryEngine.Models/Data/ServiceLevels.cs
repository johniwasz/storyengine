using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{
    public static class ServiceLevels
    {
        public static readonly Guid FreeTier = Guid.Parse("8bc0ab56-64d4-4925-8c7a-cedb070436ae");

        public static readonly Guid Designer = Guid.Parse("bdbebc49-26fd-49ef-a5d9-2a86b6b77357");

        public static readonly Guid Team = Guid.Parse("4ecc9046-fa90-474b-8ce4-58b2db559dcb");

        public static readonly Guid Enterprise = Guid.Parse("b0c5254a-3f91-415b-bdf1-8fdf25545603");
    }
}
