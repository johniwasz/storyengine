﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Google.Repository.Models
{
    public class SurfaceCapabilities
    {

        public bool HasScreen { get; set; }

        public bool HasAudio { get; set; }

        public bool HasMediaAudio { get; set; }

        public bool HasWebBrowser { get; set; }
    }
}