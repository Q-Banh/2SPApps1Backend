using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TwoSPAppsOneBackend.Schemes;

namespace TwoSPAppsOneBackend
{
    public static class Authentication
    {
        private const string authority = "https://login.microsoftonline.com/common/";
        private const string v2RedirectPath = "/v2"; //needs to be registered in the Azure App Registration in "Authentication" in Redirect URIs

        public static void AddMultiAuthentication(
          this IServiceCollection services,
          AzureAdSettings[] adSettings,
          string cookieName
        )
        {
            AzureAdSettings v1AdSettings = adSettings[0];
            AzureAdSettings v2AdSettings = adSettings[1];

            services.AddAuthentication(
                options =>
                {
                    options.DefaultScheme = AuthenticationPolicySchemes.BearerV1orV2;
                    options.DefaultChallengeScheme = AuthenticationPolicySchemes.OpenIdConnectV1orV2;
                }
              ).AddOpenIdConnect(AuthenticationSchemes.OpenIdConnectV1,
                    options =>
                    {
                        options.Authority = authority;
                        options.ClientId = v1AdSettings.ClientId;
                        options.CallbackPath = "/v1"; //this path is the RedirectUri defined in the Azure App registration in "Authentication"

                        options.SignInScheme = CookieSchemes.CookieV1;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false
                        };
                    })
              .AddOpenIdConnect(AuthenticationSchemes.OpenIdConnectV2,
                options =>
                {
                    options.Authority = authority;
                    options.ClientId = v2AdSettings.ClientId;
                    options.CallbackPath = "/v2"; //this path is the RedirectUri defined in the Azure App registration in "Authentication"

                    options.SignInScheme = CookieSchemes.CookieV1;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false
                    };
                }
              )
              .AddJwtBearer(AuthenticationSchemes.BearerV1,
                options =>
                {
                    // reference: https://github.com/Azure-Samples/active-directory-b2c-dotnetcore-webapi
                    options.Authority = authority;
                    string audience = v1AdSettings.AppIdUri;
                    options.Audience = audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidAudience = audience
                    };
                }
              )
              .AddJwtBearer(AuthenticationSchemes.BearerV2,
                options =>
                {
                    // reference: https://github.com/Azure-Samples/active-directory-b2c-dotnetcore-webapi
                    options.Authority = authority;
                    string audience = v2AdSettings.AppIdUri;
                    options.Audience = audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidAudience = audience
                    };
                }
              )
              .AddPolicyScheme(AuthenticationPolicySchemes.BearerV1orV2, AuthenticationPolicySchemes.BearerV1orV2,
                options =>
                {
                    options.ForwardDefaultSelector = context => GetBearerAuthenticationScheme(IsV1Audience(context.Request, v1AdSettings));
                }
              )
              .AddPolicyScheme(AuthenticationPolicySchemes.OpenIdConnectV1orV2, AuthenticationPolicySchemes.OpenIdConnectV1orV2,
                options =>
                {
                    options.ForwardDefaultSelector = context => GetOpenIdConnectAuthenticationScheme(IsV1Audience(context.Request, v1AdSettings));
                }
              )
              .AddPolicyScheme(CookiePolicySchemes.CookieV1OrV2, CookiePolicySchemes.CookieV1OrV2,
                options =>
                {
                    options.ForwardDefaultSelector = context => GetCookieScheme(IsV1Audience(context.Request, v1AdSettings));
                }
              )
              .AddCookie(CookieSchemes.CookieV1,
                options =>
                {
                    options.LoginPath = "/"; //depending on this path this cookie scheme will be used as registered in options.CallbackPath from OpenIdConnect-sections
                    options.Cookie.Name = cookieName;
                    options.Events.OnRedirectToAccessDenied = context => { return CatchRedirectAccessDeniedEvent(context); };
                }
              )
              .AddCookie(CookieSchemes.CookieV2,
                options =>
                {
                    options.LoginPath = v2RedirectPath; //depending on this path this cookie scheme will be used as registered in options.CallbackPath from OpenIdConnect-sections
                    options.Cookie.Name = cookieName;
                    options.Events.OnRedirectToAccessDenied = context => { return CatchRedirectAccessDeniedEvent(context); };
                }
              );
        }

        /// <summary>
        /// Override the AccessDenied event, else it will always redirect to an access denied page
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Task CatchRedirectAccessDeniedEvent(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.Headers["Location"] =
              context.RedirectUri;
            context.Response.StatusCode =
              StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Since we now have to support two Azure-CDNs with the new V2 and V1 scenario, we need to distinguish between the
        /// old app registrations (V1) and new app registrations (V2) and authenticate the user according to the "aud" in the bearer token which resembles the "AppIdUri"
        /// of the app registration
        /// </summary>
        private static bool IsV1Audience(HttpRequest request, AzureAdSettings v1Audience)
        {
            string token = ExtractBearerToken(request);
            var jwtHandler = new JwtSecurityTokenHandler();
            
            if (token != null && jwtHandler.CanReadToken(token))
            {
                string audience = jwtHandler.ReadJwtToken(token).Audiences.FirstOrDefault();
                return audience == v1Audience.AppIdUri;
            }

            if (request.Path.ToString().ToLower().EndsWith("V2"))
            { 
                return false;
            }

            return true; // If we cannot find any audience we assume we have V1 app
        }

        private static string GetBearerAuthenticationScheme(bool isV1Audience)
        {
            return isV1Audience ? AuthenticationSchemes.BearerV1 : AuthenticationSchemes.BearerV2;
        }

        private static string GetCookieScheme(bool isV1Audience)
        {
            return isV1Audience ? CookieSchemes.CookieV1 : CookieSchemes.CookieV2;
        }

        private static string GetOpenIdConnectAuthenticationScheme(bool isV1Audience)
        {
            return isV1Audience ? AuthenticationSchemes.OpenIdConnectV1 : AuthenticationSchemes.OpenIdConnectV2;
        }

        public static void UseMultiAuthentication(this IApplicationBuilder app, string cookieName)
        {
            app.UseAuthentication();
            app.Use(
              async (context, next) =>
              {
                  string token = ExtractBearerToken(context.Request);
                  bool hasCookie = context.Request.Cookies.ContainsKey(cookieName);

                  if (token == null && !hasCookie)
                  {
                      await next();
                      return;
                  }

                  if (token != null)
                  {
                      await Authenticate(context, next, AuthenticationPolicySchemes.BearerV1orV2);
                  }
                  else
                  {
                      await Authenticate(context, next, CookiePolicySchemes.CookieV1OrV2);
                  }
              }
            );
        }

        private static async Task Authenticate(HttpContext context, Func<Task> next, string schemeKey)
        {
            AuthenticateResult result = await context.AuthenticateAsync(schemeKey);
            if (result.Succeeded)
            {
                context.User = result.Principal;
                await next();
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }

        private static string ExtractBearerToken(HttpRequest request)
        {
            // parse token from header
            StringValues val;
            if (request.Headers.TryGetValue("authorization", out val) && val.Any(v => v.StartsWith("Bearer ")))
            {
                return val.First(v => v.StartsWith("Bearer ")).Split(' ').Last();
            }

            return null;
        }
    }
}
