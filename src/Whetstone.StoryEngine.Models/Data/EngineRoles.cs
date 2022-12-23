using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{
    public static class EngineRoles
    {


        public static readonly Guid ProjectAdministrator = Guid.Parse("023e1461-81ea-4272-b2ea-22c59cbe26ef");

        public static readonly Guid OrganizationAdministrator = Guid.Parse("74236635-ab8f-4f7b-bf76-b1c1089c2030");

        public static readonly Guid Developer = Guid.Parse("6d369d82-92ed-47e6-8550-c49fd01b20f3");

        public static readonly Guid ReleaseManager = Guid.Parse("a0df4f6e-d4d1-424c-a33e-73cbe9f71788");

        public static readonly Guid Reader = Guid.Parse("8b60dab8-c479-4e65-8ac8-1d816266a4d0");

        public static readonly Guid ReportViewer = Guid.Parse("4fa00695-384a-4edc-b010-06c5fad42a2a");

        public static readonly Guid MarketingManager = Guid.Parse("5097a742-e600-4372-aecc-17f365587153");
    }
}
