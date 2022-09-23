using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace CalpineAzureDashboard.Web.Controllers
{
    public class AccountController : Controller
    {
        [Route("signIn", Name = "signIn")]
        /// <summary>
        /// Send an OpenID Connect sign-in request.
        /// Alternatively, you can just decorate the SignIn method with the [Authorize] attribute
        /// </summary>
        public void SignIn()
        {
            if (!Request.IsAuthenticated) {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties {RedirectUri = "/"},
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        [Route("signOut", Name = "signOut")]
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType,CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}
