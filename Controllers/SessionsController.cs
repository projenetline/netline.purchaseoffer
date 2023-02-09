using netline.purchaseoffer.Models;
using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace netline.purchaseoffer.Controllers
{
    public class SessionsController : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Ntl_User    user =(Ntl_User) HttpContext.Current.Session["User"];


        

            var descriptor = filterContext.ActionDescriptor;
            var actionName = descriptor.ActionName;
            var controllerName = descriptor.ControllerDescriptor.ControllerName;

            if (user == null)
            {
                if (controllerName == "Offer" && (actionName == "OfferGet" || actionName == "OfferDetailView" || actionName == "FileView"))
                {

                    changeMode();


                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Index" }, { "controller", "Login" }, { "area", "" } });
                }
            }
            else
            {



                if (controllerName == "User")
                {
                    if (user.UserType == 1)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Index" }, { "controller", "Home" }, { "area", "" } });
                    }
                    else if (user.UserType == 2)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "PurchasingProjects" }, { "controller", "Confirm" }, { "area", "" } });
                    }
                    else if (user.UserType == 5)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Index" }, { "controller", "Budgets" }, { "area", "" } });
                    }
                }



                if (user.UserType == 5 && controllerName != "Budget")
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "PurchasingProjects" }, { "controller", "Budget" }, { "area", "" } });
                }
                if (user.UserType == 100)
                {
                    if (controllerName != "Home")
                    {
                        if (controllerName != "User")
                        {
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Index" }, { "controller", "Home" }, { "area", "" } });
                        }


                    }


                }
                if (user.UserType == 2)
                {




                    if (controllerName != "Confirm")
                    {



                        if ((controllerName == "Demands" && actionName != "SupplierOfferDetail"))
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "PurchasingProjects" }, { "controller", "Confirm" }, { "area", "" } });

                        if (controllerName != "Demands")
                        {
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "PurchasingProjects" }, { "controller", "Confirm" }, { "area", "" } });
                        }

                    }

                }

            }

        }


        public static void changeMode()
        {
            try
            {
                // Set the path of the config file.
                string configPath = "/Web.config";

                // Get the Web application configuration object.
                Configuration config = WebConfigurationManager.OpenWebConfiguration(configPath);

                var configuration = WebConfigurationManager.OpenWebConfiguration("/");
                var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
                if (authenticationSection.Mode == AuthenticationMode.Windows)
                {
                    authenticationSection.Mode = AuthenticationMode.None;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}