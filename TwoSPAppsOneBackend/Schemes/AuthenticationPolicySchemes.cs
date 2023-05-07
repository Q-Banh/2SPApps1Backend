namespace TwoSPAppsOneBackend.Schemes
{
    /// <summary>
    /// Available policy schemes used for authentication, switch between V1 and V2 in different containers.
    /// Source: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-6.0
    /// </summary>
    public static class AuthenticationPolicySchemes
    {
        /// <summary>
        /// Scheme to check which Bearer settings should be used old V1 or new V2
        /// </summary>
        public const string BearerV1orV2 = "Bearer_V1_or_V2";

        /// <summary>
        /// Scheme to check which OpenIdConnect settings should be used old V1 or new V2
        /// </summary>
        public const string OpenIdConnectV1orV2 = "OpenIdConnect_V1_or_V2";
    }
}
