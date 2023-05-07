namespace TwoSPAppsOneBackend.Schemes
{
    /// <summary>
    /// Available values used for authentication via Cookies, since we need to support V1 and V2 scenarios.
    /// </summary>
    public static class CookieSchemes
    {
        /// <summary>
        /// Cookie scheme for old V1
        /// </summary>
        public const string CookieV1 = "Cookie_V1";

        /// <summary>
        /// Cookie scheme for new V2
        /// </summary>
        public const string CookieV2 = "Cookie_V2";
    }
}
