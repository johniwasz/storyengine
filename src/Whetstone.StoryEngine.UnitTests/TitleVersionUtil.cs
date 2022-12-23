using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.UnitTests
{
    internal class TitleVersionUtil
    {
        internal static TitleVersion GetSymbicortTitle()
        {
            TitleVersion titleVer = new TitleVersion
            {
                ShortName = "symbicortsavings",
                Version = "0.4",
                DeploymentId = Guid.NewGuid(),
                VersionId = Guid.NewGuid()
            };
            return titleVer;
        }


        internal static TitleVersion GetWhetstoneTitle()
        {
            TitleVersion titleVer = new TitleVersion
            {
                ShortName = "whetstonetechnologies",
                Version = "0.5",
                DeploymentId = Guid.NewGuid(),
                VersionId = Guid.NewGuid()
            };
            return titleVer;
        }

        internal static TitleVersion GetWhetstoneSmsTitle()
        {
            TitleVersion titleVer = new TitleVersion
            {
                ShortName = "whetstonetechnologiessms",
                Version = "0.2",
                DeploymentId = Guid.NewGuid(),
                VersionId = Guid.NewGuid()
            };
            return titleVer;
        }

        internal static TitleVersion GetEOTEGTitle()
        {
            TitleVersion titleVer = new TitleVersion
            {
                ShortName = "eyeoftheeldergods",
                Version = "0.8",
                DeploymentId = Guid.NewGuid(),
                VersionId = Guid.NewGuid()
            };
            return titleVer;
        }


        internal static TitleVersion GetClinicalTrialTitle()
        {
            TitleVersion titleVer = new TitleVersion
            {
                ShortName = "clinicaltrialsgov",
                Version = "0.1",
                VersionId = Guid.NewGuid(),
                DeploymentId = Guid.NewGuid()
            };

            return titleVer;
        }

        internal static TitleVersion GetAnimalFarmPITitle()
        {
            TitleVersion titleVer = new TitleVersion
            {
                ShortName = "animalfarmpi",
                Version = "1.2",
                VersionId = Guid.NewGuid(),
                DeploymentId = Guid.NewGuid()
            };

            return titleVer;
        }

    }
}
