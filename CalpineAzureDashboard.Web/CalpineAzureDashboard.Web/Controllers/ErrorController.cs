using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CalpineAzureDashboard.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("error", Name = "error")]
        public ActionResult Index(string message = null)
        {
            TempData["ErrorMessage"] = message;
            return View();
        }
    }
}