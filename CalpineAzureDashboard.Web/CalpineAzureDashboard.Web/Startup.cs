﻿using System.Configuration;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(CalpineAzureDashboard.Web.Startup))]

namespace CalpineAzureDashboard.Web
{
    public partial class Startup
    {
        /// <summary>
        /// Configure OWIN to use OpenIdConnect 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            // The Client ID (a.k.a. Application ID) is used by the application to uniquely identify itself to Azure AD
            string clientId = ConfigurationManager.AppSettings["Azure:ClientId"];

            // RedirectUri is the URL where the user will be redirected to after they sign in
            string redirectUrl = ConfigurationManager.AppSettings["Azure:RedirectUri"];

            // Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or id, or 'common' for multi-tenant)
            string tenantId = ConfigurationManager.AppSettings["Azure:TenantId"];

            // Authority is the URL for authority, composed by Azure Active Directory endpoint and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com)
            string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, ConfigurationManager.AppSettings["Azure:AuthorityUri"], tenantId);


            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions {
                    // Sets the ClientId, authority, RedirectUri as obtained from web.config
                    ClientId = clientId,
                    Authority = authority,
                    RedirectUri = redirectUrl,

                    // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
                    PostLogoutRedirectUri = redirectUrl,

                    //Scope is the requested scope: OpenIdConnectScopes.OpenIdProfile is equivalent to the string 'openid profile': in the consent screen, this will result in 'Sign you in and read your profile'
                    Scope = OpenIdConnectScope.OpenIdProfile,

                    // ResponseType is set to request the id_token - which contains basic information about the signed-in user
                    ResponseType = OpenIdConnectResponseType.IdToken,

                    // ValidateIssuer set to false to allow work accounts from any organization to sign in to your application
                    // To only allow users from a single organizations, set ValidateIssuer to true and 'tenant' setting in web.config to the tenant name or Id (example: contoso.onmicrosoft.com)
                    // To allow users from only a list of specific organizations, set ValidateIssuer to true and use ValidIssuers parameter
                    TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true },

                    // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
                    Notifications = new OpenIdConnectAuthenticationNotifications {AuthenticationFailed = OnAuthenticationFailed}
                }
            );
        }

        /// <summary>
        /// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Error?message=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
