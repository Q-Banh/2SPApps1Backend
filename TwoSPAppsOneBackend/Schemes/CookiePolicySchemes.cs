namespace TwoSPAppsOneBackend.Schemes
{
    /// <summary>
    /// Available cookie policy schemes used for authentication, since we need to support V1 and V2 Azure App Registrations.
    /// Source: https://stackoverflow.com/questions/45809707/add-multiple-cookie-schemes-in-aspnet-core-2
    /// </summary>
    public static class CookiePolicySchemes
    {
        /// <summary>
        /// Scheme to check which Cookie policy should be used, V1 or V2
        /// </summary>
        public const string CookieV1OrV2 = "Cookie_V1_or_V2";
    }
}
