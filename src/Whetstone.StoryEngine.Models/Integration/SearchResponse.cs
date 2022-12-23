﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Whetstone.StoryEngine.Models.Integration
{

    /// <summary>
    /// Returned from the database search.
    /// </summary>
    public class SearchResponse : DataRetrievalResultBase
    {

        public List<Dictionary<string, dynamic>> Rows { get; set; }


        public int PageIndex { get; set; }

        public int Count { get; set; }

    }
}
