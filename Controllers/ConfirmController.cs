using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace netline.purchaseoffer.Controllers
{
    public class ConfirmController : Controller
    {
        // GET: Confirm

        ProjectUtil util = new ProjectUtil();

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Confirm(string ConfirmId)
        {
            int ProjectId = util.getConfirmProjectId(ConfirmId);
            Ntl_Request  offer = util.getResponses(ProjectId);
            return View(offer);
        }
        [SessionsController]
        public ActionResult BudgetConfirm(string ConfirmId)
        {
            int ProjectId = util.getConfirmProjectId(ConfirmId);
            Ntl_Request  offer = util.getResponses(ProjectId);
            return View(offer);
        }


        public ActionResult InvoiceConfirm(string ConfirmId)
        {

            int ProjectId = util.getInvoiceConfirmProjectId(ConfirmId);

            Ntl_BrowserOffer offer=   util.getProjectCompleted(ProjectId);

            offer.Difference = offer.InvInfo.InvoiceNetTotal - offer.OrderInfo.OrderNetTotal;

            return View(offer);
        }

        public ActionResult GetConfirm(int ProjectId)
        {
            Ntl_User user =(Ntl_User)Session["User"];
            int SuggestionSupplier=util.getSuggestionSupplier(ProjectId);

            if (SuggestionSupplier > 0)
            {
                Ntl_Request  offer = util.getResponses(ProjectId);
                Ntl_RequestSupplier SuggestionOffer=offer.RequestSuppliers.Where(x => x.SupplierRef == SuggestionSupplier).FirstOrDefault();
                List<Ntl_ConfirmList>  confirmList =util.getConfirmList(SuggestionOffer.NetTotal);
                Ntl_Confirm confirm = new Ntl_Confirm()
                {
                    Comment=new byte[]{ },
                    CommentStr="",
                    ConfirmGuid=Guid.NewGuid(),
                    ConfirmStatus=-10,
                    PersonEmail=user.Email,
                    PersonName=user.FullName,
                    ProjectId=ProjectId,
                    ConfirmNr=0
                };
                util.createConfirm(confirm);



                foreach (var item in confirmList)
                {
                    if (item.Email == "butcekontrol@raysigorta.com.tr")
                    {
                        item.FullName = "Bütçe Kontrol";
                    }
                    confirm = new Ntl_Confirm()
                    {
                        Comment = new byte[] { },
                        CommentStr = "",
                        ConfirmGuid = Guid.NewGuid(),
                        ConfirmStatus = -1,
                        PersonEmail = item.Email,
                        PersonName = item.FullName,
                        ProjectId = ProjectId,
                        ConfirmNr = item.ConfirmNr
                    };
                    util.createConfirm(confirm);

                }
                Ntl_Confirm confirmPerson = util.getConfirmPerson(ProjectId,Guid.Empty.ToString());

                if (confirmPerson != null)
                {

                    string url=string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
                    if (confirmPerson.PersonEmail == "butcekontrol@raysigorta.com.tr")
                    {
                        confirmPerson.PersonName = "Bütçe Kontrol ";
                        util.sendConfirmEmail(confirmPerson.PersonEmail, confirmPerson.ConfirmGuid, true);
                    }
                    else
                    {
                        util.sendConfirmEmail(confirmPerson.PersonEmail, confirmPerson.ConfirmGuid, false);
                    }
                }
               // util.updateConfirmStatus(ProjectId, true);
                util.updateRejectConfirmStatus(ProjectId);
            }
            return RedirectToAction("Confirm", "Demands", new { @ProjectId = ProjectId });
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult saveConfirm(string comment, string confirmId, int confirmStatus)
        {
            byte[] commentByte = Encoding.GetEncoding(1254).GetBytes(comment);
            int ProjectId=util.getConfirmProjectId(confirmId);
            util.updateConfirm(confirmId, commentByte, confirmStatus, ProjectId);



            if (confirmStatus == 1)
            {
                Ntl_Confirm confirmPerson = util.getConfirmPerson(ProjectId,confirmId);
                if (confirmPerson != null)
                {
                    string url=string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

                    if (confirmPerson.PersonEmail == "butcekontrol@raysigorta.com.tr")
                    {
                        confirmPerson.PersonName = "BÜtçe Kontrol ";
                        util.sendConfirmEmail(confirmPerson.PersonEmail, confirmPerson.ConfirmGuid, true);
                    }
                    else
                    {
                        util.sendConfirmEmail(confirmPerson.PersonEmail, confirmPerson.ConfirmGuid, false);
                    }

                }
                else
                {
                    util.sendConfirmEmail(ProjectId);

                }
            }
            else
            {
                Ntl_Confirm confirm=    util.getConfirmByGuid(confirmId);
                util.sendRejectEmail(confirm.PersonName, ProjectId);
            }

            return Json(Url.Action("Confirm", "Confirm", new { ConfirmId = confirmId }));
        }
        [SessionsController]
        public ActionResult PurchasingProjects()
        {
            Ntl_User user =(Ntl_User)Session["User"];

            Ntl_Filter filter =(Ntl_Filter)Session["Filter"];

            if (filter == null)
            {
                filter = new Ntl_Filter()
                {
                    BegDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today
                };
            }
            List<Ntl_Confirm> confirms=util.getConfirmList(user.Email,filter);
            return View(confirms);
        }
    }
}