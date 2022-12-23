﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Tracking
{
    public class InventoryItemComparer : IEqualityComparer<InventoryItemBase>
    {
        public bool Equals(InventoryItemBase x, InventoryItemBase y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(InventoryItemBase obj)
        {
            return obj.GetHashCode();
        }
    }
}
