using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;

namespace netline.purchaseoffer.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin

    
        public ActionResult Index()
        {
            return View();
        }
        jP_RestService util= new jP_RestService();
        public ActionResult ConfirmList()
        {
           

            List<Ntl_ConfirmList> confirmLists= new List<Ntl_ConfirmList>();

            return View(confirmLists);
        }
     
    }
}