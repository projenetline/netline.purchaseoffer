using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace netline.purchaseoffer.Auth.Controllers
{
    public class TeklifAuthController : Controller
    {
        // GET: TeklifAuth
        public RedirectResult Index()
        {
            return Redirect("http://localhost:1666/Login/Login?activeName=" + User.Identity.Name);
        }


        
    }
}