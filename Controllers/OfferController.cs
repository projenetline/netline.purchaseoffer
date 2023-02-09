using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
namespace netline.purchaseoffer.Controllers
{

    public class OfferController : Controller
    {
        // GET: Offer
        public ActionResult Index()
        {
            return View();
        }
        ProjectUtil util = new ProjectUtil();
        static string ErrorMessage="";
        [AllowAnonymous]
        public ActionResult OfferGet(string RequestId)
        {
            Ntl_OfferRequest request =util.getRequest(RequestId);

            if (string.IsNullOrEmpty(ErrorMessage))
                ViewBag.ErrorMessage = "";
            else
            {
                ViewBag.ErrorMessage = ErrorMessage;
                ErrorMessage = "";
            }

            Ntl_SupplierOffer offer= util.getItemsForOffer(request.ProjectId,request.SupplierRef);
            offer.RequestGuid = RequestId;
            offer.Responded = request.Responded;
            List<SelectListItem> trcurList = new List<SelectListItem>();
            trcurList.Add(new SelectListItem { Text = "USD", Value = "1" });
            trcurList.Add(new SelectListItem { Text = "EUR", Value = "20" });
            trcurList.Add(new SelectListItem { Text = "TL", Value = "160" });

            ViewBag.trcurList = trcurList;


            return View(offer);
        }

        [HttpPost]
        public ActionResult OfferGet(Ntl_SupplierOffer offer, string command)
        {
            ViewBag.ErrorMessage = "";
            if (command != "saveWithForOffer" && string.IsNullOrEmpty(offer.NotResponseExplanation))
            {
                ViewBag.ErrorMessage = "Teklif vermeme nedenini \"Açıklama\" alanına Giriniz";
                ErrorMessage = "Teklif vermeme nedenini \"Açıklama\" alanına Giriniz";
                List<SelectListItem> trcurList = new List<SelectListItem>();
                trcurList.Add(new SelectListItem { Text = "USD", Value = "1" });
                trcurList.Add(new SelectListItem { Text = "EUR", Value = "20" });
                trcurList.Add(new SelectListItem { Text = "TL", Value = "160" });

                ViewBag.trcurList = trcurList;


                return RedirectToAction("OfferGet", "Offer", new { @RequestId = offer.RequestGuid.ToString() });

            }

            ErrorMessage = "";
            Ntl_OfferRequest request =util.getRequest(offer.RequestGuid);
            if (command == "saveWithForOffer")
            {
                foreach (var line in offer.Lines)
                {
                    util.saveSupplierOffer(offer.ProjectId, line.Id, request.SupplierRef, line.NewPrice, line.Explanation, offer.RequestGuid, line.TrCurr, line.VatRate, line.TrRate);
                }
            }
            if (command == "saveWithForOffer")
            {
                util.updateRequest(offer.RequestGuid, offer.ResponseExplanation, 1);
            }
            else
            {
                util.updateRequest(offer.RequestGuid, offer.NotResponseExplanation, 2);
            }


            request = util.getRequest(offer.RequestGuid);
            offer = new Ntl_SupplierOffer();
            offer = util.getItemsForOffer(request.ProjectId, request.SupplierRef);
            offer.RequestGuid = offer.RequestGuid;
            offer.Responded = request.Responded;
            return RedirectToAction("OfferGet", "Offer", new { @RequestId = request.RequestGuid.ToString() });
        }
        [SessionsController]
        public ActionResult ManuelOfferGet(int ProjectId, int SupplierRef)
        {

            string RequestId=Guid.NewGuid().ToString();

            Ntl_OfferRequest request =new Ntl_OfferRequest();

            Ntl_SupplierOffer offer= util.getItemsForOffer(ProjectId,SupplierRef);
            offer.RequestGuid = RequestId;
            offer.Responded = request.Responded;
            List<SelectListItem> trcurList = new List<SelectListItem>();
            trcurList.Add(new SelectListItem { Text = "USD", Value = "1" });
            trcurList.Add(new SelectListItem { Text = "EUR", Value = "20" });
            trcurList.Add(new SelectListItem { Text = "TL", Value = "160" });

            ViewBag.trcurList = trcurList;


            return View(offer);
        }
        [HttpPost]
        [SessionsController]
        public ActionResult ManuelOfferGet(Ntl_SupplierOffer offer)
        {
            int requestNr=0;
            Ntl_OfferRequest request= util.getRequestGuid(offer.SupplierRef,offer.ProjectId);
            if (request == null)
            {
                request = new Ntl_OfferRequest()
                {
                    ProjectId = offer.ProjectId,
                    RequestDate_ = DateTime.Today,
                    RequestGuid = Guid.Parse(offer.RequestGuid),
                    Responded = 1,
                    SupplierRef = offer.SupplierRef
                };
                requestNr = util.getNextRequestNr(offer.ProjectId, offer.SupplierRef);
            }
            else
            {
                requestNr = request.RequestNr;
            }

            util.saveSupplierRequest(offer.ProjectId, offer.SupplierRef, request.RequestGuid, requestNr);
            foreach (var line in offer.Lines)
            {
                util.saveSupplierOffer(offer.ProjectId, line.Id, request.SupplierRef, line.NewPrice, line.Explanation, request.RequestGuid.ToString(), line.TrCurr, line.VatRate, line.TrRate);
            }
            util.updateRequest(request.RequestGuid.ToString(), offer.Explanation, 1);
            request = util.getRequest(request.RequestGuid.ToString());
            offer = new Ntl_SupplierOffer();
            offer = util.getItemsForOffer(request.ProjectId, request.SupplierRef);
            offer.RequestGuid = offer.RequestGuid;
            offer.Responded = request.Responded;
            return RedirectToAction("OfferGet", "Offer", new { @RequestId = request.RequestGuid.ToString() });
        }
        public ActionResult OfferDetail(string RequestId)
        {
            Ntl_OfferRequest request =util.getRequest(RequestId);

            List<Ntl_OfferDetail> detail= util.getOfferDetail(request.ProjectId);
            Ntl_PersonInfo person=util.getProjectRespnsible(request.ProjectId);
            ViewBag.ProjectResp = $@"<div class=""pull-right"" > <b  style="" border-bottom: 2px solid currentColor;"" >Proje Sorumlusu : {person.PersonName}</b></hr> </br>
<b  style="" border-bottom: 2px solid currentColor;"" >E-Posta : {person.PersonEmail}</b></hr> </br>
<b  style="" border-bottom: 2px solid currentColor;"" >Telefon : {person.Phone1}</b></hr> </br> </div>";

            return View(detail);
        }
        public ActionResult OfferDetailView(int ProjectId)
        {

            List<Ntl_OfferDetail> detail= util.getOfferDetail(ProjectId);

            return View(detail);
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




    }
}