namespace TwoSPAppsOneBackend.Schemes
{
    /// <summary>
    /// Available values used for authentication, since we need to support V1 and V2.
    /// </summary>
    public static class AuthenticationSchemes
    {
        /// <summary>
        /// Bearer authentication for old V1
        /// </summary>
        public const string BearerV1 = "Bearer_V1";

        /// <summary>
        /// OpenIdConnect authentication for old V1
        /// </summary>
        public const string OpenIdConnectV1 = "OpenIdConnect_V1";

        /// <summary>
        /// Bearer authentication for new V2
        /// </summary>
        public const string BearerV2 = "Bearer_V2";

        /// <summary>
        /// OpenIdConnect authentication for new V2
        /// </summary>
        public const string OpenIdConnectV2 = "OpenIdConnect_V2";
    }
}
