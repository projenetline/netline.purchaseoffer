using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace netline.purchaseoffer.Controllers
{
    [SessionsController]
    public class UserController : Controller
    {
        // GET: User
        ProjectUtil util= new ProjectUtil();
        [SessionsController]
        public ActionResult Index()
        {
            List<Ntl_User> users= util.getUsers();
            return View(users);
        }
        [SessionsController]
        public ActionResult EditUser(int Id)
        {
            List<SelectListItem> UserTypelist = new List<SelectListItem>();
            UserTypelist.Add(new SelectListItem { Text = "Satınalma Kullanıcısı", Value = "1" });
            UserTypelist.Add(new SelectListItem { Text = "Onay Kullanıcısı", Value = "2" });
            UserTypelist.Add(new SelectListItem { Text = "Bütçe Kullanıcısı", Value = "5" });
            UserTypelist.Add(new SelectListItem { Text = "Administrator", Value = "100" });
            
        
            ViewBag.UserTypelist = UserTypelist;

            Ntl_User user= util.getUser(Id);
            return View(user);
        }
        [SessionsController]
        [HttpPost]
        public ActionResult EditUser(Ntl_User user)
        {
            util.UpdateUser(user);
            return RedirectToAction("Index");
        }
        [SessionsController]
        public ActionResult NewUser()
        {
            List<SelectListItem> UserTypelist = new List<SelectListItem>();
            UserTypelist.Add(new SelectListItem { Text = "Satınalma Kullanıcısı", Value = "1" });
            UserTypelist.Add(new SelectListItem { Text = "Onay Kullanıcısı", Value = "2" });
            UserTypelist.Add(new SelectListItem { Text = "Bütçe Kullanıcısı", Value = "5" });
            UserTypelist.Add(new SelectListItem { Text = "Administrator", Value = "100" });

         
            ViewBag.UserTypelist = UserTypelist;

            Ntl_User user=new Ntl_User();
            user.FullName = "Yeni Kullanıcı";
            return View(user);
        }

        [HttpPost]
        public ActionResult NewUser(Ntl_User user)
        {
            util.SaveUser(user);
            return RedirectToAction("Index");
        }
    }
}