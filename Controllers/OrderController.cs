using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace netline.purchaseoffer.Controllers
{
    [SessionsController]
    public class OrderController : Controller
    {
        ProjectUtil util=new ProjectUtil();
        [SessionsController]
        public ActionResult Index(int ProjectId)
        {

            Ntl_SupplierOrder order = util.getOrder(ProjectId);
            if (order.DocList.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    order.DocList.Add(new Ntl_OrderDocs());
                }
            }

            foreach (var line in order.Lines)
            {
                line.NetTotal = (line.Price * line.TrRate * line.Quantity) * ((line.VatRate / 100) + 1);
                order.NetTotal += line.NetTotal;
            }

            return View(order);
        }
        [SessionsController]
        public ActionResult SendOrder(int ProjectId)
        {

            Ntl_SupplierOrder order = util.getOrder(ProjectId);

            



            if (order.DocList.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    order.DocList.Add(new Ntl_OrderDocs());
                }
            }

            foreach (var line in order.Lines)
            {
                line.NetTotal = (line.Price * line.TrRate * line.Quantity) * ((line.VatRate / 100) + 1);
                order.NetTotal += line.NetTotal;
            }

            return View(order);
        }





        [SessionsController]
        public ActionResult SendToSupplier(int ProjectId, int SupplierRef)
        {
            return RedirectToAction("SendOrder", "Order", new { ProjectId });
        }


        [SessionsController]
        [HttpPost]
        public ActionResult SendOrder(Ntl_SupplierOrder order, string command)
        {
            int ProjectId=order.ProjectId;
            int SupplierRef = order.SupplierRef;
            if (command == "SendSupplier")
            {

                util.saveSupplierOrder(order);

                order = util.getOrder(order.ProjectId);
                if (order.DocList.Count == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        order.DocList.Add(new Ntl_OrderDocs());
                    }
                }

                foreach (var line in order.Lines)
                {
                    line.NetTotal = (line.Price * line.TrRate * line.Quantity) * ((line.VatRate / 100) + 1);
                    order.NetTotal += line.NetTotal;
                }


                string ExcelPath= util.createSupplierExcel(ProjectId);
                if (util.sendSupplierOrderEmail("", SupplierRef, ProjectId, ExcelPath))
                {

                    util.updateSupplierOrder(SupplierRef, ProjectId);

                    if (order.DocList.Count == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            order.DocList.Add(new Ntl_OrderDocs());
                        }
                    }

                    foreach (var line in order.Lines)
                    {
                        line.NetTotal = (line.Price * line.TrRate * line.Quantity) * ((line.VatRate / 100) + 1);
                        order.NetTotal += line.NetTotal;
                    }
                }

            }
            else if (command == "saveOrder")
            {
                util.saveSupplierOrder(order);
                util.updateOrderDocs(order.ProjectId);



                order = util.getOrder(order.ProjectId);
                if (order.DocList.Count == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        order.DocList.Add(new Ntl_OrderDocs());
                    }
                }

                foreach (var line in order.Lines)
                {
                    line.NetTotal = (line.Price * line.TrRate * line.Quantity) * ((line.VatRate / 100) + 1);
                    order.NetTotal += line.NetTotal;
                }

            }
            return RedirectToAction("SendOrder", "Order", new { ProjectId = ProjectId, SupplierRef = SupplierRef });
        }



        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file, string ProjectId)
        {

            Ntl_OrderDocsUploadResult uploadResult= new Ntl_OrderDocsUploadResult();
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
                    string path=  Server.MapPath("~")+ "FileOrderFolder\\"+prjNr+"";
                    bool exists = System.IO.Directory.Exists(path);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(path);

                    System.IO.File.WriteAllBytes(path + "\\" + file.FileName, fileData);
                    if (util.saveOrderDocs(prjId, file.FileName, file.ContentType, path + "\\" + file.FileName) > 0)
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
            uploadResult.Docs = util.getOrderDocs(Convert.ToInt32(ProjectId));

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
        public FileResult FileView(int DocumentId)
        {
            Ntl_OfferDocs offerDoc=util.getOrderDocument(DocumentId);
            byte[] fileBytes = System.IO.File.ReadAllBytes(offerDoc.UploadedFilePath);
            return File(fileBytes, offerDoc.UploadedFileContentTyp);
        }



    }
}

