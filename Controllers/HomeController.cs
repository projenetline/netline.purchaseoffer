using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace netline.purchaseoffer.Controllers
{
    public class HomeController : Controller
    {
        ProjectUtil util = new ProjectUtil();
        [SessionsController]
        public ActionResult Index()
        {

            Ntl_User user =(Ntl_User)Session["User"];
            Ntl_Filter filter =(Ntl_Filter)Session["Filter"];
            List<SelectListItem> transferMonthlist = new List<SelectListItem>();
            transferMonthlist.Add(new SelectListItem { Text = "Ocak", Value = "1" });
            transferMonthlist.Add(new SelectListItem { Text = "Şubat", Value = "2" });
            transferMonthlist.Add(new SelectListItem { Text = "Mart", Value = "3" });
            transferMonthlist.Add(new SelectListItem { Text = "Nisan", Value = "4" });
            transferMonthlist.Add(new SelectListItem { Text = "Mayıs", Value = "5" });
            transferMonthlist.Add(new SelectListItem { Text = "Haziran", Value = "6" });
            transferMonthlist.Add(new SelectListItem { Text = "Temmuz", Value = "7" });
            transferMonthlist.Add(new SelectListItem { Text = "Ağustos", Value = "8" });
            transferMonthlist.Add(new SelectListItem { Text = "Eylül", Value = "9" });
            transferMonthlist.Add(new SelectListItem { Text = "Ekim", Value = "10" });
            transferMonthlist.Add(new SelectListItem { Text = "Kasım", Value = "11" });
            transferMonthlist.Add(new SelectListItem { Text = "Aralık", Value = "12" });
            ViewBag.transferMonthlist = transferMonthlist;
            if (filter == null)
            {
                filter = new Ntl_Filter()
                {
                     BegDate=DateTime.Today.AddMonths(-1),
                     EndDate=DateTime.Today
                };
            }

            List<Ntl_Confirm> confirms=util.getConfirmList(user.Email,filter);
            return View(confirms);
        }


    }
}