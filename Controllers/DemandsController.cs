using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace netline.purchaseoffer.Controllers
{
    public class DemandsController : Controller
    {
        ProjectUtil util = new ProjectUtil();
        [SessionsController]
        public ActionResult Index()
        {
            List<Ntl_Demand> demandList= util.getDemandList();
            Ntl_Demands demands = new Ntl_Demands();
            demands.Demands = demandList;
            List<SelectListItem> groupList = new List<SelectListItem>();
            foreach (var item in util.getGroupList())
            {
                groupList.Add(new SelectListItem { Text = item, Value = item });

            }



            ViewBag.ItemGroupList = groupList;
            return View(demands);
        }

        [SessionsController]
        public ActionResult RequirementDemands()
        {
            List<Ntl_Demand> demandList= util.getRequirementDemandList();
            Ntl_Demands demands = new Ntl_Demands();
            demands.Demands = demandList;
            List<SelectListItem> groupList = new List<SelectListItem>();
            foreach (var item in util.getGroupList())
            {
                groupList.Add(new SelectListItem { Text = item, Value = item });

            }



            ViewBag.ItemGroupList = groupList;
            return View(demands);
        }

        [SessionsController]
        public ActionResult NewOffers(string transRefs)
        {
            Ntl_Offer offer= new Ntl_Offer();
            int prNr=util.getNextProjectNr()+1;


            offer.ProjectNr = "Prj-" + prNr.ToString().PadLeft(5, '0');
            offer.Lines = util.getOfferLines(transRefs);

            foreach (var item in offer.Lines)
            {
                item.Total = item.LastPurchPrice * item.Quantity;
                item.NetTotal = item.Total + ((item.Total / 100) * item.VatRate);
            }
            List<int> suppliers = new List<int>();
            if (Session["SuppList"] != null)
            {
                suppliers = (List<int>)Session["SuppList"];
            }


            foreach (var item in suppliers)
            {
                offer.Suppliers.Add(util.getSupplierByRef(item));
            }



            return View(offer);
        }
        [SessionsController]
        public ActionResult WaitingOffer(int ProjectId, int typ)
        {
            Ntl_Offer offer = util.getWaitingOffer(ProjectId);
            ViewBag.ProjectId = ProjectId;
            List<int> suppliers = new List<int>();

            ViewBag.Typ = typ;
            foreach (var supplier in offer.Suppliers)
            {
                suppliers.Add(supplier.SupplierRef);
            }
            Session["SuppList"] = suppliers;
            return View(offer);
        }
        [SessionsController]
        public ActionResult CancelOffer(int ProjectId)
        {
            util.CancelOffer(ProjectId);
            return RedirectToAction("Index");
        }
        [SessionsController]
        public ActionResult OfferDetail(int ProjectId, int OfferNr)
        {
            Ntl_Offer offer = util.getWaitingOffer(ProjectId,OfferNr);
            offer.OfferNr = OfferNr;
            return View(offer);
        }
        [SessionsController]
        public ActionResult SupplierOfferDetail(int ProjectId, int OfferNr, int supplierRef)
        {
            Ntl_Offer offer = util.getSupplierWaitingOffer(ProjectId,OfferNr,supplierRef);
            offer.OfferNr = OfferNr;
            return View(offer);
        }
        [HttpPost]
        [SessionsController]
        public ActionResult WaitingOffer(Ntl_Offer offer, string command)
        {

            int ProjectId=   util.saveOffer(offer,0);



            if (command == "saveDemand")
            {
                List<int> offerSuppliers=(List<int>)Session["SuppList"];
                foreach (var offerSupplier in offerSuppliers)
                {
                    Ntl_OfferSupplier ntl_OfferSupplier= util.getSupplierByRef(offerSupplier);
                    util.saveOfferSupplier(ntl_OfferSupplier, ProjectId);

                }
            }

            if (command == "sendForOffer")
            {
                List<int> suppliers = new List<int>();
                suppliers = (List<int>)Session["SupplierForRequest"];
                if (suppliers == null)
                {
                    suppliers = new List<int>();
                    foreach (var item in offer.Suppliers)
                    {
                        if (!suppliers.Contains(item.SupplierRef))
                        {
                            suppliers.Add(item.SupplierRef);
                        }
                    }
                }

                int requestNr=util.getNextRequestNr(ProjectId);
                foreach (var item in suppliers.Distinct())
                {
                    string mail= util.getSupplierMail(item);
                    Guid newRequestGuid= Guid.NewGuid();
                    util.saveSupplierRequest(offer.ProjectId, item, newRequestGuid, requestNr);
                    string url=string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
                    util.sendEmailService(mail, newRequestGuid, url);
                }

                util.sendOfferNotifyEmail(offer.ProjectId);
                return RedirectToAction("WaitingOffer", "Demands", new { @ProjectId = offer.ProjectId, @typ = 1 });
            }
            return RedirectToAction("WaitingOffer", "Demands", new { @ProjectId = offer.ProjectId, @typ = 1 });


        }
        [HttpPost]
        [SessionsController]
        public ActionResult NewOffers(Ntl_Offer offer)
        {
            int ProjectId=0;
            Ntl_User user= (Ntl_User)Session["User"];
            int prNr=util.getNextProjectNr()+1;
            offer.ProjectNr = "Prj-" + prNr.ToString().PadLeft(5, '0');

            foreach (var line in offer.Lines)
            {
                foreach (var transRef in line.TransRef)
                {
                    if (util.transRefControl(transRef))
                    {
                        ProjectId = util.transRefProjectId(transRef);
                        return RedirectToAction("WaitingOffer", "Demands", new { @ProjectId = ProjectId, @typ = 1 });
                    }
                }
            }


            ProjectId = util.saveOffer(offer, user.Id);
            foreach (var line in offer.Lines)
            {
                int Id= util.saveOfferLine(line,ProjectId);
                foreach (var transRef in line.TransRef)
                {
                    util.saveLineTrans(transRef, Id);
                }
            }
            foreach (var supplier in offer.Suppliers)
            {
                int supplierId = util.saveOfferSupplier(supplier,ProjectId);
            }
            return RedirectToAction("WaitingOffer", "Demands", new { @ProjectId = ProjectId, @typ = 1 });
        }
        [SessionsController]
        public ActionResult Confirm(int ProjectId)
        {
            double Eur=0;
            double Usd=0;
            try
            {
                DataSet dsDovizKur = new DataSet();
                dsDovizKur.ReadXml(@"https://www.tcmb.gov.tr/kurlar/today.xml");
                DataRow drUsd = dsDovizKur.Tables[1].Rows[0];
                DataRow drEur = dsDovizKur.Tables[1].Rows[3];

                Eur = Convert.ToDouble(drEur[4].ToString().Replace('.', ','));
                Usd = Convert.ToDouble(drUsd[4].ToString().Replace('.', ','));


            }
            catch (Exception)
            {


            }


            util.updateProjectTrRate(ProjectId, Eur, Usd);
            Ntl_Request offers = util.getResponses(ProjectId);




            List<SelectListItem> List = new List<SelectListItem>();
            //  List.Add(new SelectListItem { Text ="Netline", Value = "Netline" });
            foreach (var item in util.getDepartments(ProjectId))
            {
                if (!string.IsNullOrEmpty(item.name))
                    List.Add(new SelectListItem { Text = item.name, Value = item.value });

            }

            List<SelectListItem> PersonList = new List<SelectListItem>();
            PersonList.Add(new SelectListItem { Text = "", Value = "" });

            ViewBag.PersonList = PersonList;
            ViewBag.DepertmentList = List;




            return View(offers);
        }
        [SessionsController]
        public ActionResult SendForOffer(int ProjectId)
        {


            return RedirectToAction("WaitingOffer", "Demands", new { ProjectId = ProjectId, @typ = 1 });
        }
        public FileResult FileView(int DocumentId)
        {
            Ntl_OfferDocs offerDoc=util.getDocument(DocumentId);
            byte[] fileBytes = System.IO.File.ReadAllBytes(offerDoc.UploadedFilePath);
            if (offerDoc.UploadedFileContentTyp.Contains("image") || offerDoc.UploadedFileContentTyp.Contains("pdf"))
            {
                return File(fileBytes, offerDoc.UploadedFileContentTyp);
            }
            else
            {
                return File(fileBytes, offerDoc.UploadedFileContentTyp, offerDoc.UploadedFileName);
            }
        }
        [SessionsController]
        public ActionResult DocView(int DocumentId)
        {
            Ntl_OfferDocs offerDoc=util.getDocument(DocumentId);



            return View(offerDoc);
        }
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file, string ProjectId)
        {

            Ntl_OfferDocsUploadResult uploadResult= new Ntl_OfferDocsUploadResult();
            try
            {



                string extension = Path.GetExtension(file.FileName).ToLower();

                if (extension == ".txt" || extension == ".xml" || extension == ".xlsx" || extension == ".xls" || extension == ".pdf" || extension == ".doc" || extension == ".png" || extension == ".jpeg" || extension == ".docx" || extension == ".jpg")
                {

                    int prjId=Convert.ToInt32(ProjectId);
                    string prjNr=util.getProjeNo(prjId);
                    var memStream = new MemoryStream();
                    file.InputStream.CopyTo(memStream);
                    byte[] fileData = memStream.ToArray();
                    string path=  Server.MapPath("~")+ "FileFolder\\"+prjNr+"";
                    bool exists = System.IO.Directory.Exists(path);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    System.IO.File.WriteAllBytes(path + "\\" + file.FileName, fileData);
                    if (util.saveProjectDocs(prjId, file.FileName, file.ContentType, path + "\\" + file.FileName) > 0)
                    {
                        uploadResult.Result = "Dosya Yüklendi.";
                        uploadResult.Success = true;
                    }
                }
                else
                {
                    uploadResult.Result = "Dosya formatı uygun değildir.";
                    uploadResult.Success = false;
                }
            }
            catch (Exception ex)
            {

                uploadResult.Result = ex.Message;
                uploadResult.Success = false;
            }
            uploadResult.Docs = util.getOfferDocs(Convert.ToInt32(ProjectId));


            return Json(uploadResult, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult DeleteFile(string FileName)
        {



            return Json("", JsonRequestBehavior.AllowGet);
        }
        [SessionsController]
        public ActionResult OfferView(int ProjectId)
        {


            Ntl_SupplierOffer offer= util.getItemsForOffer(ProjectId,0);
            Ntl_PersonInfo person=util.getProjectRespnsible(ProjectId);


            ViewBag.ProjectResp = $@"<div class=""pull-right"" > <b  style="" border-bottom: 2px solid currentColor;"" >Proje Sorumlusu : {person.PersonName}</b></hr> </br>
<b  style="" border-bottom: 2px solid currentColor;"" >E-Posta : {person.PersonEmail}</b></hr> </br>
<b  style="" border-bottom: 2px solid currentColor;"" >Telefon : {person.Phone1}</b></hr> </br> </div>";


            return View(offer);
        }
        [SessionsController]
        public ActionResult OfferDetailView(int ProjectId)
        {

            List<Ntl_OfferDetail> detail= util.getOfferDetail(ProjectId);

            return View(detail);
        }
        [SessionsController]
        public ActionResult ProjectOnOffer(string pageNr)
        {

            List<Ntl_BrowserOffer> offerList=util.getProjectsOnOffer(Convert.ToInt32(pageNr));

            foreach (var offer in offerList)
            {
                Ntl_ProjectStatus projectStatus = util.getProjectStatus(offer.ProjectId);

                if (!string.IsNullOrEmpty(projectStatus.CommentPerson))
                {
                    offer.Status_ = "Yorum Bekleniyor";
                }
                else if (!string.IsNullOrEmpty(projectStatus.ConfirmPerson))
                {
                    offer.Status_ = "Onay Bekleniyor";
                }
                else if (!string.IsNullOrEmpty(projectStatus.RejectPerson))
                {
                    offer.Status_ = "Onaylanmadan geri geldi";
                }
                else
                {
                    if (projectStatus.ConfirmedPersonel > 0)
                    {
                        offer.Status_ = "Onaylandı";
                    }
                    else
                    {
                        offer.Status_ = "Bekliyor";

                    }
                }
            }
            int totalpage = 0;
            int TotalOffer=util.getCountProjectsOnOffer();
            if (TotalOffer % 15 > 0)
            {
                totalpage = (TotalOffer / 15) + 1;
            }
            else
            {
                totalpage = (TotalOffer / 15);
            }



            int pagenr=Convert.ToInt16(pageNr);
            string pagination = $" <div class=\"fixed-table-pagination\"> ";
            pagination += "  <div class=\"pull-left pagination-detail\">";
            if (TotalOffer < 15)
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else if (TotalOffer < (pagenr * 15))
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else
            { pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + (pagenr * 15) + " arası listeleniyor</span>"; }
            pagination += "  </div>";

            pagination += "    <div class=\"pull-right pagination\">";
            pagination += "      <ul class=\"pagination\">";

            if (pagenr > 1)
            {
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectOnOffer?pageNr=1' > &lt; &lt;</a></li>";
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectOnOffer?pageNr=1' > &lt;</a></li>";

            }
            else
            {
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectOnOffer?pageNr=1' >  &lt; &lt;</a></li>";
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectOnOffer?pageNr=" + (pagenr - 1) + "' > &lt;</a></li>";
            }

            if (pagenr == 1)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == 1)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectOnOffer?pageNr=" + i + "'  > " + i + " </a></li>";
                    }
                    else
                    {
                        if (i <= totalpage)
                        {

                            pagination += " <li><a class=\"page-number\"  href='/Demands/ProjectOnOffer?pageNr=" + i + "'   > " + i + " </a></li>";
                        }

                    }
                }

            }
            else if (pagenr == 2)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectOnOffer?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectOnOffer?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";

                    //}
                }

            }
            else
            {
                for (int i = pagenr - 2; i < pagenr + 3; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectOnOffer?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectOnOffer?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";
                    //}
                }
            }
            if (pagenr == totalpage)
            {
                pagination += "  <li class=\"disabled\" ><a href='/Demands/ProjectOnOffer?pageNr=" + pagenr + "'>&gt;</a></li>";
                pagination += "  <li  class=\"disabled\" ><a href='/Demands/ProjectOnOffer?pageNr=" + totalpage + "' disabled > &gt; &gt;</a></li>";
            }
            else
            {
                pagination += "  <li><a href='/Demands/ProjectOnOffer?pageNr=" + (pagenr + 1) + "'  > &gt;</a></li>";
                pagination += "  <li><a href='/Demands/ProjectOnOffer?pageNr=" + (totalpage) + "' > &gt; &gt;</a></li>";
            }


            pagination += "  </ul>";
            pagination += "  </div>";
            pagination += "  </div>";

            ViewBag.Pagination = pagination;



            return View(offerList);

        }
        [SessionsController]
        public ActionResult ProjectWaitinOrder(string pageNr)
        {
            Dictionary<string, string> ProjectWaitinOrder= new Dictionary<string, string>();
            ProjectWaitinOrder.Add("PrjNo", "");
            ProjectWaitinOrder.Add("ProjectStatus", "");

            if (Session["ProjectWaitinOrder"] != null)
            {
                ProjectWaitinOrder = (Dictionary<string, string>)Session["ProjectWaitinOrder"];
            }

            List<Ntl_BrowserOffer> offerList=util.getProjectsWaitingOrder(Convert.ToInt32(pageNr),ProjectWaitinOrder);
            foreach (var offer in offerList)
            {
                Ntl_ProjectStatus projectStatus = util.getProjectStatus(offer.ProjectId);
                offer.Supplier = util.getSupplierByRef(offer.SuggestionSupplierRef);
                if (offer.ProjectStatus == 1)
                {
                    offer.Status_ = "Yorum Bekleniyor";
                }
                else if (offer.ProjectStatus == 2)
                {
                    offer.Status_ = "Onay Bekleniyor (" + projectStatus.ConfirmPerson + ")";
                }
                else if (offer.ProjectStatus == 3)
                {
                    offer.Status_ = "Onaylanmadan geri geldi. (" + projectStatus.RejectPerson + ")";
                }
                else if (offer.ProjectStatus == 4)
                {
                    offer.Status_ = "Onaylandı";
                }
                else
                {
                    offer.Status_ = "Bekliyor";
                }



            }


            List<SelectListItem> drpStatusList = new List<SelectListItem>();
            drpStatusList.Add(new SelectListItem { Text = "Hepsi", Value = "0" });
            drpStatusList.Add(new SelectListItem { Text = "Yorum Bekleniyor", Value = "1" });
            drpStatusList.Add(new SelectListItem { Text = "Onay Bekleniyor", Value = "2" });
            drpStatusList.Add(new SelectListItem { Text = "Onaylanmadan geri geldi", Value = "3" });
            drpStatusList.Add(new SelectListItem { Text = "Onaylandı", Value = "4" });
            ViewBag.drpStatusList = drpStatusList;

            int totalpage = 0;
            int TotalOffer=util.getCountProjectsWaitingOrder(ProjectWaitinOrder);
            if (TotalOffer % 15 > 0)
            {
                totalpage = (TotalOffer / 15) + 1;
            }
            else
            {
                totalpage = (TotalOffer / 15);
            }



            int pagenr=Convert.ToInt16(pageNr);
            string pagination = $" <div class=\"fixed-table-pagination\"> ";
            pagination += "  <div class=\"pull-left pagination-detail\">";
            if (TotalOffer < 15)
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 15) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else if (TotalOffer < (pagenr * 15))
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else
            { pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + (pagenr * 15) + " arası listeleniyor</span>"; }
            pagination += "  </div>";

            pagination += "    <div class=\"pull-right pagination\">";
            pagination += "      <ul class=\"pagination\">";

            if (pagenr > 1)
            {
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectWaitinOrder?pageNr=1' > &lt; &lt;</a></li>";
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectWaitinOrder?pageNr=1' > &lt;</a></li>";

            }
            else
            {
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectWaitinOrder?pageNr=1' >  &lt; &lt;</a></li>";
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectWaitinOrder?pageNr=" + (pagenr - 1) + "' > &lt;</a></li>";
            }

            if (pagenr == 1)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == 1)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectWaitinOrder?pageNr=" + i + "'  > " + i + " </a></li>";
                    }
                    else
                    {
                        if (i <= totalpage)
                        {

                            pagination += " <li><a class=\"page-number\"  href='/Demands/ProjectWaitinOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                        }

                    }
                }

            }
            else if (pagenr == 2)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectWaitinOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectWaitinOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";

                    //}
                }

            }
            else
            {
                for (int i = pagenr - 2; i < pagenr + 3; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectWaitinOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectWaitinOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";
                    //}
                }
            }
            if (pagenr == totalpage)
            {
                pagination += "  <li class=\"disabled\" ><a href='/Demands/ProjectWaitinOrder?pageNr=" + pagenr + "'>&gt;</a></li>";
                pagination += "  <li  class=\"disabled\" ><a href='/Demands/ProjectWaitinOrder?pageNr=" + totalpage + "' disabled > &gt; &gt;</a></li>";
            }
            else
            {
                pagination += "  <li><a href='/Demands/ProjectWaitinOrder?pageNr=" + (pagenr + 1) + "'  > &gt;</a></li>";
                pagination += "  <li><a href='/Demands/ProjectWaitinOrder?pageNr=" + (totalpage) + "' > &gt; &gt;</a></li>";
            }


            pagination += "  </ul>";
            pagination += "  </div>";
            pagination += "  </div>";

            ViewBag.Pagination = pagination;

            return View(offerList);


        }
        [SessionsController]
        public ActionResult ProjectOnOrder(string pageNr)
        {
            Dictionary<string, string> ProjectOnOrder= new Dictionary<string, string>();
            ProjectOnOrder.Add("PrjNo", "");
            ProjectOnOrder.Add("OrderNr", "");
            ProjectOnOrder.Add("Supplier", "");
            ProjectOnOrder.Add("EndDate", "");

            if (Session["ProjectOnOrder"] != null)
            {
                ProjectOnOrder = (Dictionary<string, string>)Session["ProjectOnOrder"];
            }



            List<Ntl_BrowserOffer> offerList=util.getProjectsOnOrder(Convert.ToInt32(pageNr),ProjectOnOrder);


            int totalpage = 0;
            int TotalOffer=util.getCountProjectsOnOrder(ProjectOnOrder);

            if (TotalOffer % 15 > 0)
            {
                totalpage = (TotalOffer / 15) + 1;
            }
            else
            {
                totalpage = (TotalOffer / 15);
            }



            int pagenr=Convert.ToInt16(pageNr);
            string pagination = $" <div class=\"fixed-table-pagination\"> ";
            pagination += "  <div class=\"pull-left pagination-detail\">";
            if (TotalOffer < 15)
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else if (TotalOffer < (pagenr * 15))
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else
            { pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + (pagenr * 14) + " arası listeleniyor</span>"; }
            pagination += "  </div>";

            pagination += "    <div class=\"pull-right pagination\">";
            pagination += "      <ul class=\"pagination\">";

            if (pagenr > 1)
            {
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectOnOrder?pageNr=1' > &lt; &lt;</a></li>";
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectOnOrder?pageNr=1' > &lt;</a></li>";

            }
            else
            {
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectOnOrder?pageNr=1' >  &lt; &lt;</a></li>";
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectOnOrder?pageNr=" + (pagenr - 1) + "' > &lt;</a></li>";
            }

            if (pagenr == 1)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == 1)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectOnOrder?pageNr=" + i + "'  > " + i + " </a></li>";
                    }
                    else
                    {
                        if (i <= totalpage)
                        {

                            pagination += " <li><a class=\"page-number\"  href='/Demands/ProjectOnOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                        }

                    }
                }

            }
            else if (pagenr == 2)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectOnOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectOnOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";

                    //}
                }

            }
            else
            {
                for (int i = pagenr - 2; i < pagenr + 3; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectOnOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectOnOrder?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";
                    //}
                }
            }
            if (pagenr == totalpage)
            {
                pagination += "  <li class=\"disabled\" ><a href='/Demands/ProjectOnOrder?pageNr=" + pagenr + "'>&gt;</a></li>";
                pagination += "  <li  class=\"disabled\" ><a href='/Demands/ProjectOnOrder?pageNr=" + totalpage + "' disabled > &gt; &gt;</a></li>";
            }
            else
            {
                pagination += "  <li><a href='/Demands/ProjectOnOrder?pageNr=" + (pagenr + 1) + "'  > &gt;</a></li>";
                pagination += "  <li><a href='/Demands/ProjectOnOrder?pageNr=" + (totalpage) + "' > &gt; &gt;</a></li>";
            }


            pagination += "  </ul>";
            pagination += "  </div>";
            pagination += "  </div>";

            ViewBag.Pagination = pagination;


            return View(offerList);


        }
        [SessionsController]
        public ActionResult ProjectsCompleted(string pageNr)
        {
            Dictionary<string, string> ProjectsCompleted= new Dictionary<string, string>();
            ProjectsCompleted.Add("PrjNo", "");
            ProjectsCompleted.Add("OrderNr", "");
            ProjectsCompleted.Add("Supplier", "");
            ProjectsCompleted.Add("InvoiceNr", "");
            ProjectsCompleted.Add("EndDate", "");
            if (Session["ProjectsCompleted"] != null)
            {
                ProjectsCompleted = (Dictionary<string, string>)Session["ProjectsCompleted"];
            }



            List<Ntl_BrowserOffer> offerList=util.getProjectsCompleted(Convert.ToInt32(pageNr),ProjectsCompleted);
            foreach (var offer in offerList)
            {
                if (offer.InvInfo != null && offer.OrderInfo != null)
                {
                    offer.InvoiceControl = true;
                    if (offer.InvInfo.InvoiceNetTotal - offer.OrderInfo.OrderNetTotal > 50)
                    {
                        offer.InvoiceControl = true;
                        offer.Difference = offer.InvInfo.InvoiceNetTotal - offer.OrderInfo.OrderNetTotal;
                    }
                }
                if (offer.InvInfo == null)
                {

                    offer.InvInfo = new Ntl_InvoiceInfo()
                    {
                        InvoiceNetTotal = 0,
                        InvoiceNr = "",
                        InvoiceSlipRef = 0,
                        InvoiceStatus = 0


                    };
                }
                if (offer.OrderInfo == null)
                {

                    offer.OrderInfo = new Ntl_OrderInfo()
                    {

                    };
                }
            }

            int totalpage = 0;
            int TotalOffer=util.getCountProjectsCompleted(ProjectsCompleted);
            if (TotalOffer % 15 > 0)
            {
                totalpage = (TotalOffer / 15) + 1;
            }
            else
            {
                totalpage = (TotalOffer / 15);
            }



            int pagenr=Convert.ToInt16(pageNr);
            string pagination = $" <div class=\"fixed-table-pagination\"> ";
            pagination += "  <div class=\"pull-left pagination-detail\">";
            if (TotalOffer < 15)
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else if (TotalOffer < (pagenr * 15))
            {
                pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + TotalOffer + " arası listeleniyor</span>";
            }
            else
            { pagination += "    <span class=\"pagination-info\">" + TotalOffer + " Kayıttan " + ((pagenr * 15) - 14) + " ile " + (pagenr * 15) + " arası listeleniyor</span>"; }
            pagination += "  </div>";

            pagination += "    <div class=\"pull-right pagination\">";
            pagination += "      <ul class=\"pagination\">";

            if (pagenr > 1)
            {
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectsCompleted?pageNr=1' > &lt; &lt;</a></li>";
                pagination += "           <li class=\"page-pre\"><a href='/Demands/ProjectsCompleted?pageNr=1' > &lt;</a></li>";

            }
            else
            {
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectsCompleted?pageNr=1' >  &lt; &lt;</a></li>";
                pagination += "           <li class=\"disabled page-pre\"><a href='/Demands/ProjectsCompleted?pageNr=" + (pagenr - 1) + "' > &lt;</a></li>";
            }

            if (pagenr == 1)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == 1)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectsCompleted?pageNr=" + i + "'  > " + i + " </a></li>";
                    }
                    else
                    {
                        if (i <= totalpage)
                        {

                            pagination += " <li><a class=\"page-number\"  href='/Demands/ProjectsCompleted?pageNr=" + i + "'   > " + i + " </a></li>";
                        }

                    }
                }

            }
            else if (pagenr == 2)
            {
                for (int i = 1; i < 6; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectsCompleted?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectsCompleted?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";

                    //}
                }

            }
            else
            {
                for (int i = pagenr - 2; i < pagenr + 3; i++)
                {
                    if (i == pagenr)
                    {
                        pagination += " <li class=\"page-number active\"><a  href='/Demands/ProjectsCompleted?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    else if (i <= totalpage)
                    {

                        pagination += " <li><a class=\"page-number\" href='/Demands/ProjectsCompleted?pageNr=" + i + "'   > " + i + " </a></li>";
                    }
                    //else
                    //{
                    //    pagination += " <li><a  class=\"disabled\" href='javascript:;' onclick=\"getTicketList('" + i + "')\" > " + i + " </a></li>";
                    //}
                }
            }
            if (pagenr == totalpage)
            {
                pagination += "  <li class=\"disabled\" ><a href='/Demands/ProjectsCompleted?pageNr=" + pagenr + "'>&gt;</a></li>";
                pagination += "  <li  class=\"disabled\" ><a href='/Demands/ProjectsCompleted?pageNr=" + totalpage + "' disabled > &gt; &gt;</a></li>";
            }
            else
            {
                pagination += "  <li><a href='/Demands/ProjectsCompleted?pageNr=" + (pagenr + 1) + "'  > &gt;</a></li>";
                pagination += "  <li><a href='/Demands/ProjectsCompleted?pageNr=" + (totalpage) + "' > &gt; &gt;</a></li>";
            }


            pagination += "  </ul>";
            pagination += "  </div>";
            pagination += "  </div>";

            ViewBag.Pagination = pagination;

            return View(offerList);


        }
        [SessionsController]
        public ActionResult ConfirmInvoice(int InvoiceRef, int ProjectId)
        {
            util.UpdateInvoice(InvoiceRef);
            util.setInvoiceStatus(ProjectId);
            util.updateBudgetBlokeAmount(ProjectId);


            return RedirectToAction("ProjectsCompleted", "Demands");
        }
        [SessionsController]
        public ActionResult NewDemands(string transRefs)
        {
            Ntl_User user=(Ntl_User)Session["User"];
            Ntl_Anly anly =util.GetAnlyInfo(user.AnlyCode);
            Ntl_Talep offer= new Ntl_Talep();
            offer.ProjeId = util.getNextTalepProjectNr() + 1;


            offer.ProjeKodu = "Tlp-" + offer.ProjeId.ToString().PadLeft(8, '0');
            offer.Lines = util.getDemandLines(transRefs);

            foreach (var line in offer.Lines)
            {
                line.AnalizBoyutuKodu = anly.Code;
                line.AnalizBoyutuRef = anly.Logicalref;
                line.AnalizBoyutuAdi = anly.Description;
            }
            return View(offer);


        }
        [SessionsController]
        [HttpPost]
        public ActionResult NewDemands(Ntl_Talep talep)
        {
            Ntl_User user=(Ntl_User)Session["User"];

            Ntl_Anly anly =util.GetAnlyInfo(user.AnlyCode);


            if (util.createPurchFlow(talep.ProjeId, talep.ProjeKodu) > 0)
            {

                foreach (var line in talep.Lines)
                {
                    string refs="";
                    foreach (var transRef in line.Transrefs)
                    {
                        refs += "," + transRef;
                    }

                    refs = refs.Substring(1);


                    Ntl_Talep_Detay newLine = util.getDemandLine(refs);

                    newLine.Miktar = line.Miktar;
                    newLine.ProjeKodu = talep.ProjeKodu;
                    newLine.Transrefs = line.Transrefs;
                    newLine.AnalizBoyutuKodu = anly.Code;
                    newLine.AnalizBoyutuRef = anly.Logicalref;
                    newLine.AnalizBoyutuAdi = anly.Description;
                    util.createPurchFlowLines(newLine, talep.ProjeId);
                }


                return RedirectToAction("RequirementDemands", "Demands");
            }
            return View(talep);


        }
        [SessionsController]
        public ActionResult CompleteReqDemands()
        {
            List<Ntl_BrwsrTalep> brwsrTaleps=util.GetBrwsrTaleps();



            return View(brwsrTaleps);


        }
        [SessionsController]
        public ActionResult CompleteReqDemandsDetail(string DemandNr)
        {
            Ntl_Talep talep=util.getCompletedRequirementDemand(DemandNr);
            return View(talep);


        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult saveProjectComment(string comment, string projectId)
        {
            Ntl_User user= (Ntl_User)Session["User"];
            byte[] commentByte = Encoding.GetEncoding(1254).GetBytes(comment);
            Ntl_Comment ntl_Comment= new Ntl_Comment()
            {
                CommentGuid=Guid.NewGuid(),
                PersonEmail=user.Email,
                PersonName=user.FullName,
                Comment=commentByte,
                ProjectId=Convert.ToInt32( projectId),
                Status_=true,
                CommentTime=DateTime.Now,

            };
            util.createComment(ntl_Comment);

            return Json(Url.Action("Confirm", "Demands", new { @ProjectId = ntl_Comment.ProjectId }));
        }

    }
}
