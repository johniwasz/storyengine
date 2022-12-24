namespace Whetstone.StoryEngine.Security.Claims
{
    public static class SoniBridgeClaimTypes
    {
        public static readonly string SoniBridgeIssuer = "LOCAL AUTHORITY";

        public static readonly string Organization = "http://schemas.sonibridge.org/2019/06/identity/claims/organization";

        public static readonly string AccountStatus = "http://schemas.sonibridge.org/2019/06/identity/claims/accountstatus";

        public static readonly string SubscriptionLevel = "http://schemas.sonibridge.org/2019/06/identity/claims/subscriptionlevel";

        public static readonly string Permission = "http://schemas.sonibridge.org/2019/06/identity/claims/permission";
    }
}
