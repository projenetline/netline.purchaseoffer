using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System.Web.Mvc;

namespace netline.purchaseoffer.Controllers
{

    public class LoginController : Controller
    {
        ProjectUtil util = new ProjectUtil();
        static  string UserID="";
        static bool isLogout=false;
        public ActionResult Index()
        { 
            Ntl_User user=new Ntl_User();
            user.ActiveName = User.Identity.Name;
            return View(user);
        }  

        public ActionResult Auth()
        {
          

            Ntl_User user = util.getUserByActiveName( User.Identity.Name);
            if (user != null)
            {
                Session["Filter"] = null;
                Session["User"] = user;
                if (user.ResetPassword)
                {
                    return RedirectToAction("NewPassword", "Login", new { Id = user.Id });
                }

                if (user.UserType == 5)
                {
                    return RedirectToAction("Index", "Budget");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]

        public ActionResult Index(Ntl_User user)
        {
            string activeName=user.ActiveName;
            if (user == null || user.Id == 0)
            {
                user = util.getUser(user.UserName, user.Password);
                if (user == null)
                {
                    ViewBag.msg = " Kullanıcı Adı veya Şifre Yanlış ";
                    user = new Ntl_User();
                    UserID = "";
                    return View(user);
                }
                else
                {
                    Session["Filter"] = null;
                    Session["User"] = user;
                    UserID = "";

                    util.UpdateActiveName(user.Id, activeName);
                    if (user.ResetPassword)
                    {
                        return RedirectToAction("NewPassword", "Login", new { Id = user.Id });
                    }


                    if (user.UserType == 5)
                    {
                        return RedirectToAction("Index", "Budget");
                    }

                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                Session["Filter"] = null;
                Session["User"] = user;
                if (user.ResetPassword)
                {
                    return RedirectToAction("NewPassword", "Login", new { Id = user.Id });
                }


                if (user.UserType == 5)
                {
                    return RedirectToAction("Index", "Budget");
                }

                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session["User"] = null;
            Session["Filter"] = null;
            isLogout = true;
            UserID = "";
            return RedirectToAction("Index");
        }
        public ActionResult NewPassword(int Id)
        {

            Ntl_User user = new Ntl_User();
            user.Id = Id;
            return View(user);
        }

        [HttpPost]
        public ActionResult NewPassword(Ntl_User user)
        {
            if (user.Password == user.ConfirmPassword)
            {

                if (util.UpdatePassword(user.Id, user.Password) > 0)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.msg = " Şifreleriniz güncellenemdi. ";
                }

            }
            else
            {

                ViewBag.msg = " Şifrelerinizi aynı giriniz ";
            }



            return View(user);
        }
    }
}