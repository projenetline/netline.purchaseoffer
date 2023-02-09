using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace netline.purchaseoffer.Auth.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            string activeName=User.Identity.Name;

            ViewBag.Title = "Home Page" + activeName;

            return View();
        }
    }
}
