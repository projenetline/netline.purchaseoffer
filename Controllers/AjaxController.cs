using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;


namespace netline.purchaseoffer.Controllers
{
    [SessionsController]
    public class AjaxController : Controller
    {
        // GET: Ajax

        ProjectUtil util = new ProjectUtil();

        [SessionsController]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult SendInvoiceConfirm(int ProjectId, string Explanation)
        {

            Ntl_InvoiceConfirm confirm=new Ntl_InvoiceConfirm()
            {
                ProjectId=ProjectId,
                Comment=new byte[]{ },
                CommentStr="",
                ConfirmGuid= Guid.NewGuid(),
                ConfirmStatus=0,
                PersonEmail="butcekontrol@raysigorta.com.tr",
                PersonName="Bütçe Kontrol "  ,
                ProjectNo=util.getProjeNo(ProjectId),
                Explanation=Explanation

            };

            util.createInvoiceConfirm(confirm);
            return Json(Url.Action("ProjectsCompleted", "Demands"));

        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult confirmInvoice(string ConfirmGuid)
        {
            util.updateInvoiceConfirm(ConfirmGuid);
            return Json(Url.Action("PurchasingProjects", "Budget"));

        }


        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult saveProjectExplanation(string projectId, string explanation)
        {

            util.updateProjectExp(projectId, explanation);

            return Json(Url.Action("Confirm", "Demands", new { @ProjectId = projectId }));
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult saveProjectOfferExplanation(string projectId, string explanation)
        {

            util.updateProjectOfferExp(projectId, explanation);

            return Json(Url.Action("Confirm", "Demands", new { @ProjectId = projectId }));
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult updateProjectSupplier(int SuggestionSupplierRef, int ProjectId)
        {
            util.updateProjectSupplier(SuggestionSupplierRef, ProjectId);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult sendAsOrder(int ProjectId, int SupplierRef)
        {
            string res="";
            string data="";
            try
            {
                int OrderSlipRef=0;
                jP_RestService jP_Rest = new jP_RestService();
                data = getOrderJsonString(ProjectId, SupplierRef);
                string prjNo=util.getProjeNo(ProjectId);
                jP_Rest.getToken();
                res = jP_Rest.postOrder(data, prjNo);


                if (int.TryParse(res, out OrderSlipRef))
                {
                    util.UpdateOfferStatus(ProjectId, OrderSlipRef);
                    util.insertInvoiceBudget(OrderSlipRef);

                    List<Ntl_BudgetReturn> budgetReturns=  util.GetBudgetReturns(ProjectId);
                    foreach (var budgetReturn in budgetReturns)
                    {
                        if (budgetReturn.isContract)
                        {
                            double amount = util.getProjectYearAmount(ProjectId);
                            if (amount > 0)
                                budgetReturn.NetTotal = amount;
                        }

                        util.updateBudgetBlokeAmount(budgetReturn);
                    }
                }
                else
                {
                    util.updateTransferMessage(ProjectId, res);

                }
            }
            catch (Exception ex)
            {

                res = ex.Message;
            }


            return Json(res, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult SetSupplierSession(int SupplierRef, int add)
        {
            List<int> suppliers = new List<int>();
            if (Session["SuppList"] != null)
            {
                suppliers = (List<int>)Session["SuppList"];
            }



            if (!suppliers.Contains(SupplierRef) && add == 1)
            {
                suppliers.Add(SupplierRef);
            }
            else if (suppliers.Contains(SupplierRef) && add == 0)
            {
                suppliers.Remove(SupplierRef);
            }

            Session["SuppList"] = suppliers;

            return Json("", JsonRequestBehavior.AllowGet);
        }





        private string getOrderJsonString(int projectId, int SupplierRef)
        {
            Ntl_OfferForOrder offer = util.getWaitingOfferForOrder(projectId,SupplierRef);

            string slipNr= util.getNextOrderNr();

            Ntl_Order order= new Ntl_Order();
            ArpOfReceipt arpOfReceipt=new ArpOfReceipt()
            {
                aRPOfReceipt_Code=offer.Supplier.SupplierCode,
                reference=SupplierRef,

            };


            order.arpOfReceipt = arpOfReceipt;
            ArpRef  arpRef= new ArpRef()
            {
                reference=SupplierRef,
                aRP_Code=offer.Supplier.SupplierCode,
            };
            order.arpRef = arpRef;
            order.deductionPart1 = 2;
            order.deductionPart2 = 3;
            //    order.departmentRef.reference = 2;

            DepartmentFirmRef departmentFirmRef= new DepartmentFirmRef()
            {
                department_Firm_Companynr=1,
                department_Firm_DomainId=0,
                reference=0,

            };
            DepartmentRef departmentRef= new DepartmentRef()
            {
                department_Code="",
                reference=2,
                department_DomainId=0,
                department_FirmRef=departmentFirmRef
            };
            order.departmentRef = departmentRef;


            DivisionFirmRef divisionFirmRef = new DivisionFirmRef()
            {
                division_Firm_Companynr=1,
                division_Firm_DomainId=0,
                reference=1
            };
            DivisionRef divisionRef= new DivisionRef()
            {
                division_Code="1",
                reference=1,
                division_DomainId=0,
                division_FirmRef=divisionFirmRef
            };
            order.divisionRef = divisionRef;


            order.orderDate = DateTime.Now;
            order.slipNumber = slipNr;
            order.slipType = 1;

            SourceWHFirmRef sourceWHFirmRef = new SourceWHFirmRef()
            {
                reference=1,
                sourceWH_Firm_Companynr=1,
                sourceWH_Firm_DomainId=0,
            };
            SourceWHRef sourceWHRef= new SourceWHRef()
            {
                reference=348,
                sourceWH_Code="1.8",
                sourceWH_DomainId=0,
                sourceWH_FirmRef=sourceWHFirmRef,



            };
            order.sourceWHRef = sourceWHRef;







            DivisionFirmRef2 divisionFirmRef2 = new DivisionFirmRef2()
            {
                division_Firm_Companynr=1,
                division_Firm_DomainId=0,
                reference=1
            };
            DivisionRef2 divisionRef2= new DivisionRef2()
            {
                division_Code="1",
                reference=1,
                division_DomainId=0,
                division_FirmRef=divisionFirmRef2
            };
            DepartmentFirmRef2 departmentFirmRef2= new DepartmentFirmRef2()
            {
                department_Firm_Companynr=1,
                department_Firm_DomainId=0,
                reference=0,

            };
            DepartmentRef2 departmentRef2= new DepartmentRef2()
            {
                department_Code="",
                reference=2,
                department_DomainId=0,
                department_FirmRef=departmentFirmRef2
            };
            List<Item> items= new List<Item>();
            foreach (var line in offer.Lines)
            {


                Item item= new Item();


                ARPRef2  arpRef2= new ARPRef2()
                {
                    reference=SupplierRef,
                    aRP_Code=offer.Supplier.SupplierCode,
                };
                item.aRPRef = arpRef2;
                Ntl_ItemInfo itemInfo= util.GetItemInfo(line.ItemRef);

                int cardType=util.getItemCardType(line.ItemRef);
                if (cardType == 30)
                {
                    item.transType = 4;
                }



                item.departmentRef = departmentRef2;


                item.detailLineId = 2;
                item.divisionRef = divisionRef2;

                item.dueDate = DateTime.Now;




                List<Order_ExtraProp> extraProps= new List<Order_ExtraProp>();
                Order_ExtraProp order_Extra= new Order_ExtraProp()
                {
                    name="Code",
                    value=itemInfo.ItemCode
                };
                extraProps.Add(order_Extra);
                order_Extra = new Order_ExtraProp()
                {
                    name = "resolver-name",
                    value = "item"
                };
                extraProps.Add(order_Extra);
                Order_MasterReference master= new Order_MasterReference()
                {
                    reference=line.ItemRef,
                    extraProps=extraProps
                };


                item.master_Reference = master;



                OrderSlipRef orderSlipRef=new OrderSlipRef()
                {
                    orderSlip_SlipNumber= slipNr,
                    reference=0,
                    orderSlip_SlipType=1
                };

                item.orderSlipRef = orderSlipRef;

                item.price = line.Priceses[0].Price;
                item.quantity = line.Quantity;
                item.slipDate = DateTime.Now;
                item.slipOrder = 1;
                item.slipType = 1;
                SourceWHFirmRef2 sourceWHFirmRef2 = new SourceWHFirmRef2()
                {
                    reference=1,
                    sourceWH_Firm_Companynr=1,
                    sourceWH_Firm_DomainId=0,
                };
                SourceWHRef2 sourceWHRef2= new SourceWHRef2()
                {
                    reference=348,
                    sourceWH_Code="1.8",
                    sourceWH_DomainId=0,
                    sourceWH_FirmRef=sourceWHFirmRef2,



                };

                item.sourceWHRef = sourceWHRef2;

                item.total = line.Priceses[0].Price * line.Quantity;
                item.netTotal = line.Priceses[0].Price * line.Quantity;
                item.unitSetRef = itemInfo.UomSetRef;
                item.uOMDivisor = 1;
                item.uOMMultiplier = 1;
                UOMUomsetref uomsetref = new UOMUomsetref()
                {
                    reference=itemInfo.UomSetRef,
                    uOM_Uomsetref_Code=itemInfo.UnitSetCode,

                };
                Order_UOMRef uOMRef= new Order_UOMRef()
                {
                    reference=itemInfo.UomRef,
                    uOM_Code=itemInfo.UnitCode,
                    uOM_Uomsetref=uomsetref


                };
                item.uOMRef = uOMRef;


                item.vATAmount = (item.netTotal = line.Priceses[0].Price * line.Quantity) * 0.18;
                item.vATBase = item.netTotal = line.Priceses[0].Price * line.Quantity;
                item.vATRate = 18;


                ADDetailLines aDDetailLines= new ADDetailLines();

                OUFirmRef oUFirmRef= new OUFirmRef()
                {
                    oU_Firm_Companynr=1,
                    oU_Firm_DomainId=0,
                    reference=1
                };

                OURef oURef= new OURef()
                {
                    oU_Code="1",
                    oU_DomainId=0,
                    oU_FirmRef=oUFirmRef,
                    reference=1 ,


                };
                SlipRef slipRef= new SlipRef()
                {
                    slip_SlipNumber=slipNr,
                    slip_SlipType=1,

                };

                Ntl_Anly anly= util.GetAnly(line.Id);
                AnalysisDimRef analysisDim= new AnalysisDimRef()
                {
                    analysisDim_Code =anly.Code,
                    reference=anly.Logicalref


                };

                Item2 ItemAnly=new Item2()
                {
                    analysisDimRef=analysisDim,
                    distributionRatio=100,
                    lCNet=0,
                    lineNr=1,
                    oURef=oURef,
                    quantity=line.Quantity,
                    slipDate=DateTime.Now,
                    slipRef=slipRef,
                    slipType=1,
                    sourceWHRef=350,
                    unitSetRef=itemInfo.UomSetRef,
                    uOMDivisor=1,
                    uOMMultiplier=1,
                    uOMRef =itemInfo.UomRef,
                };


                List<Item2> ItemAnlyList= new List<Item2>();


                ItemAnlyList.Add(ItemAnly);



                aDDetailLines.items = ItemAnlyList;








                item.aDDetailLines = aDDetailLines;


                items.Add(item);
            }


            Transactions transactions= new Transactions();



            transactions.items = items;
            order.transactions = transactions;

            double total =0;
            double totalVat=0;
            foreach (var item in items)
            {
                total += item.netTotal;
                totalVat += item.vATAmount;
            }

            order.totalDiscounted = total;
            order.totalGross = total;
            order.totalVat = totalVat;
            order.totalNet = total + totalVat;



            string jsonStr= JsonConvert.SerializeObject(order);


            return jsonStr;

        }



        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult GetSuppliers(string SupplierCode, string SupplierDesc)
        {

            List<Ntl_OfferSupplier> Suppliers=util.getSuppliers(SupplierCode,SupplierDesc);

            List<int> suppliers = new List<int>();
            if (Session["SuppList"] != null)
            {
                suppliers = (List<int>)Session["SuppList"];
            }
            foreach (var item in Suppliers)
            {
                if (suppliers.Contains(item.SupplierRef))
                {

                    item.selectSupplier = true;
                }
            }


            return Json(Suppliers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult SelectSupplier(int projectId)
        {
            List<int> offerSuppliers=  util.getSupplierRefListByOrderId(projectId);


            List<int> suppliers = new List<int>();
            if (Session["SuppList"] != null)
            {
                suppliers = (List<int>)Session["SuppList"];
            }


            List<Ntl_OfferSupplier> Suppliers=new List<Ntl_OfferSupplier>();
            List<int>  SessionSuppliers =new List<int>();

            foreach (var item in suppliers)
            {
                if (!offerSuppliers.Contains(item))
                {
                    Ntl_OfferSupplier Supplier=util.getSupplierByRef(item);
                    Suppliers.Add(Supplier);
                }

                SessionSuppliers.Add(item);

            }
            Session["SuppList"] = SessionSuppliers;



            return Json(Suppliers, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult SaveDocumentName(string docName, string docId, string ProjectId)
        {
            int Id=Convert.ToInt32(docId.Replace(',','.'));

            util.UpdateOfferDoc(Id, docName);
            List<Ntl_OfferDocs> docList= util.getOfferDocs(Convert.ToInt32(ProjectId));
            return Json(docList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult SaveOrderDocumentName(string docName, string docId, string ProjectId)
        {
            int Id=Convert.ToInt32(docId.Replace(',','.'));
            util.UpdateOrderDoc(Id, docName);
            List<Ntl_OrderDocs> docList= util.getOrderDocs(Convert.ToInt32(ProjectId));
            return Json(docList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult DeleteDocument(string docId, string ProjectId)
        {
            int Id=Convert.ToInt32(docId.Replace(',','.'));

            util.DeleteOfferDoc(Id);
            List<Ntl_OfferDocs> docList= util.getOfferDocs(Convert.ToInt32(ProjectId));
            return Json(docList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult DeleteOrderDocument(string docId, string OrderId)
        {
            int Id=Convert.ToInt32(docId.Replace(',','.'));

            util.DeleteOrderDoc(Id);
            List<Ntl_OrderDocs> docList= util.getOrderDocs(Convert.ToInt32(OrderId));
            return Json(docList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getDocument(string ProjectId)
        {
            List<Ntl_OfferDocs> docList= util.getOfferDocs(Convert.ToInt32(ProjectId));
            return Json(docList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getOrderDocument(string ProjectId)
        {
            List<Ntl_OrderDocs> docList= util.getOrderDocs(Convert.ToInt32(ProjectId));
            return Json(docList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult SetSuppliers(List<int> Suppliers)
        {


            Session["SupplierForRequest"] = Suppliers;

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult UpdateCommentPerson(string mail,string person, int projectId)
        {


            util.UpdateCommentPerson(mail, person, projectId);

            return Json("", JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult BudgetControl(string ProjectId, string SupplierRef)
        {

            List<Ntl_BudgetControl> budgetCtrl=  util.getItemForBudget(ProjectId);

            List<Ntl_BudgetResponse> responses= new List<Ntl_BudgetResponse>();
            foreach (var item in budgetCtrl)
            {
                Ntl_BudgetResponse response = new Ntl_BudgetResponse();
                if (item.NetTotal <= item.Budget.Budget)
                {
                    response.BudgetInfo = "<strong style=\"color:forestgreen\"> " + item.Budget.BudgetName + " bütçe kalemi için uygundur. </strong> ";
                    response.BudgetOk = true;
                    response.BudgetCode = item.Budget.BudgetCode;
                }
                else
                {
                    response.BudgetInfo = "<strong  style=\"color:red\">" + item.Budget.BudgetName + " bütçe kalemi için ayrılan bütçe  " + item.Budget.Budget.ToString("N2") + " TL dir.  </strong>";
                    response.BudgetOk = false;
                    response.BudgetCode = item.Budget.BudgetCode;
                }

                responses.Add(response);

            }
            List <Ntl_BudgetResponse> ress=new List<Ntl_BudgetResponse>();

            foreach (var item in responses)
            {
                if (!ress.Select(x => x.BudgetCode).ToList().Contains(item.BudgetCode))
                {
                    ress.Add(item);
                }
            }

            return Json(ress, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult GetDemands(Ntl_DemandFilter ntl_DemandFilter)
        {
            List<Ntl_Demand> demandList= util.getDemandList(ntl_DemandFilter);

            if (demandList == null)
                demandList = new List<Ntl_Demand>();

            return Json(demandList, JsonRequestBehavior.AllowGet);


        }



        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult GetRequirementDemands(Ntl_DemandFilter ntl_DemandFilter)
        {
            List<Ntl_Demand> demandList= util.getRequirementDemandList(ntl_DemandFilter);
            return Json(demandList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult CreateOffer(List<string> transrefList)
        {
            Session["SuppList"] = new List<int>();
            string transRefs= "";

            foreach (var transref in transrefList)
            {
                transRefs += "," + transref;
            }

            transRefs = transRefs.Substring(1);
            return Json(Url.Action("NewOffers", "Demands", new { @transRefs = transRefs }));
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult CreateDemand(List<string> transrefList)
        {
            Session["SuppList"] = new List<int>();
            string transRefs= "";

            foreach (var transref in transrefList)
            {
                transRefs += "," + transref;
            }

            transRefs = transRefs.Substring(1);
            return Json(Url.Action("NewDemands", "Demands", new { @transRefs = transRefs }));
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getDepartmentPerson(string department)
        {
            List<Ntl_SelectValue> persons= util.getDepartmentPersons(department);

            var list =util.getConfirmList();
            foreach (var item in list)
            {
                persons.Add(new Ntl_SelectValue() {
                  name = item.FullName,
                   value=item.Email
                });
            }



            //persons.Add(new Ntl_SelectValue() { name = "İsmail Özer", value = "ismail.ozer@netline.net.tr" });
            return Json(persons, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getProjectStatus(int ProjectId)
        {
            Ntl_ProjectStatus projectStatus = util.getProjectStatus(ProjectId);


            if (projectStatus.SuggestionSupplierRef == 0)
            {
                projectStatus.ProjectStatus = "Bir tedarikçi seçmeniz gerekiyor.";
            }
            else if (projectStatus.PaymentPlan == 0)
            {
                projectStatus.ProjectStatus = "Ödeme planı Girmeniz gerekiyor.";
            }
            else
            {
                projectStatus.ProjectStatus = "Yorum istemelisiniz veya onaya göndermelisiniz.";
                if (!string.IsNullOrEmpty(projectStatus.CommentPerson))
                {
                    projectStatus.ProjectStatus = projectStatus.CommentPerson + " adlı personelin yorumu bekleniyor.";
                }
                else if (!string.IsNullOrEmpty(projectStatus.ConfirmPerson))
                {
                    if (projectStatus.ConfirmPerson == "Bütçe Kontrol")
                    {
                        projectStatus.ProjectStatus = "Kontrol ve Planlama departmanından bütçe onayı bekleniyor.";
                    }
                    else
                    {
                        projectStatus.ProjectStatus = projectStatus.ConfirmPerson + " adlı personelin onayi bekleniyor.";
                    }
                }
                else if (projectStatus.RejectPerson != "")
                {

                    projectStatus.ProjectStatus = projectStatus.RejectPerson + " adlı personel onaylamadan geri gönderdi.";
                }
                else
                {
                    if (projectStatus.ConfirmedPersonel > 0)
                    {
                        projectStatus.ProjectStatus = "Proje Onaylandi.";

                    }
                }
            }

            return Json(projectStatus, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getPayPlan(int ProjectId)
        {
            List<Ntl_Payplan> payplans = util.getPayplans(ProjectId);

            foreach (var payplan in payplans)
            {
                payplan.AmountStr = payplan.Amount.ToString("N2");
            }

            return Json(payplans, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult savePlan(int ProjectId, double amount1, double amount2, double amount3, double amount4, double amount5, double amount6)
        {
            List<Ntl_Payplan> payplans = new List<Ntl_Payplan>();

            util.savePayplans(ProjectId, 1, amount1);
            util.savePayplans(ProjectId, 2, amount2);
            util.savePayplans(ProjectId, 3, amount3);
            util.savePayplans(ProjectId, 4, amount4);
            util.savePayplans(ProjectId, 5, amount5);
            util.savePayplans(ProjectId, 6, amount6);

            return Json("", JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getProjectStatusForOrder(int ProjectId)
        {
            string Result="";
            string  taxnr = util.getProjectStatusForOrder(ProjectId);

            if (string.IsNullOrEmpty(taxnr))
            {
                Result = "Tedarikçinin Vergi Kimlik Numarası girilmemiştir.";

            }
            else
            {
                if (util.getSupplierStatus(taxnr))
                {
                    Result = "Tedarikçi kontrol listesini geçememiştir.";
                }

            }

            return Json(Result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getLastestCurr()
        {
            Ntl_Currency currency = new Ntl_Currency();
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    DataSet dsDovizKur = new DataSet();
                    dsDovizKur.ReadXml(@"https://www.tcmb.gov.tr/kurlar/today.xml");
                    DataRow drUsd = dsDovizKur.Tables[1].Rows[0];
                    DataRow drEur = dsDovizKur.Tables[1].Rows[3];
                    currency.Eur = drEur[4].ToString().Replace('.', ',');
                    currency.Usd = drUsd[4].ToString().Replace('.', ',');
                }


            }
            catch (Exception es)
            {

                currency = new Ntl_Currency();
            }
            return Json(currency, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getPriceStr(string price)
        {


            double dbl_price= Convert.ToDouble(price.Replace('.',','));


            string value=  dbl_price.ToString("N3");
            return Json(value, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult setProjectListFilter(string BegDate, string EndDate, string supplier)
        {

            Ntl_Filter filter = new Ntl_Filter()
            {
                BegDate=Convert.ToDateTime(BegDate),
                EndDate=Convert.ToDateTime(EndDate),
                Supplier= supplier

            };
            Session["Filter"] = filter;


            return Json("", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getOfferExplain(string requestGuid)
        {
            string explain=util.getOfferExplainByRequestGuid(requestGuid);
            return Json(explain, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult sendReminder(string requestId)
        {

            Ntl_OfferRequest request=   util.getRequest(requestId);
            int ProjectId=  util.getRequestProjectId(requestId);



            Guid newRequestGuid=Guid.Parse(requestId);


            int requestNr=util.getNextRequestNr(ProjectId);


            string mail= util.getSupplierMail(request.SupplierRef);



            string url=string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            //  util.InsertErrorLogs("sendReminder", "Mail Gönderilecek ", mail + " " + request.SupplierRef);

            util.sendReminderEmailService(mail, newRequestGuid, url);


            return RedirectToAction("WaitingOffer", "Demands", new { @ProjectId = request.ProjectId });
        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getHistory(string projectId)
        {

            List<Ntl_ConfirmHistory> historyList=util.getHistory(Convert.ToInt32(projectId));
            foreach (var history in historyList)
            {
                history.TimeStr = history.Time_.ToString("HH.mm");
                history.DateStr = history.Time_.ToString("dd.MM.yyyy");
                if (history.BeforeTime_.Year == 1900)
                {

                    history.Duration = "";
                }
                else
                {


                    double minutes =   history.Time_.Subtract(history.BeforeTime_).TotalMinutes;
                    int day=0;
                    int saat=0;
                    if (minutes > 1440)
                    {
                        day = (int)minutes / 1440;

                        minutes = minutes - (day * 1440);
                    }
                    if (minutes > 60)
                    {
                        saat = (int)minutes / 60;
                        minutes = minutes - (saat * 60);
                    }
                    int dakika=(int)minutes%60;

                    if (day > 0)
                    {
                        history.Duration += day + " Gün ";
                    }
                    if (saat > 0)
                    {
                        history.Duration += saat + " Saat ";
                    }

                    history.Duration += dakika + " Dakika ";
                }
            }

            return Json(historyList, JsonRequestBehavior.AllowGet);

        }



        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult removeOffers(int offerNr, int projectId, string Exp)
        {
            util.removeOffers(offerNr, projectId);
            Ntl_User user= (Ntl_User)Session["User"];
            util.deleteOfferInfoEmail(projectId, 0, offerNr, user.Id, Exp);

            return Json(Url.Action("WaitingOffer", "Demands", new { @ProjectId = projectId, @typ = 1 }));

        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult removeOffer(int offerNr, int supplierId, int projectId, string Exp)
        {
            util.removeOffer(offerNr, supplierId, projectId);
            Ntl_User user= (Ntl_User)Session["User"];
            util.deleteOfferInfoEmail(projectId, supplierId, offerNr, user.Id, Exp);
            return Json(Url.Action("WaitingOffer", "Demands", new { @ProjectId = projectId, @typ = 1 }));
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getProjectOnOrder(string PrjNo, string OrderNr, string Supplier, string EndDate)
        {

            Dictionary<string, string> ProjectOnOrder = new Dictionary<string, string>();


            ProjectOnOrder.Add("PrjNo", PrjNo);
            ProjectOnOrder.Add("OrderNr", OrderNr);
            ProjectOnOrder.Add("Supplier", Supplier);
            ProjectOnOrder.Add("EndDate", EndDate);



            Session["ProjectOnOrder"] = ProjectOnOrder;


            return Json(Url.Action("ProjectOnOrder", "Demands", new { @pageNr = "1" }));
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getProjectWaitinOrder(string PrjNo, string ProjectStatus)
        {

            Dictionary<string, string> ProjectWaitinOrder = new Dictionary<string, string>();
            ProjectWaitinOrder.Add("PrjNo", PrjNo);
            ProjectWaitinOrder.Add("ProjectStatus", ProjectStatus);       



            Session["ProjectWaitinOrder"] = ProjectWaitinOrder;


            return Json(Url.Action("ProjectWaitinOrder", "Demands", new { @pageNr = "1" }));
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getProjectsCompleted(string PrjNo, string OrderNr, string Supplier, string InvoiceNr, string EndDate)
        {

            Dictionary<string, string> ProjectsCompleted = new Dictionary<string, string>();


            ProjectsCompleted.Add("PrjNo", PrjNo);
            ProjectsCompleted.Add("OrderNr", OrderNr);
            ProjectsCompleted.Add("Supplier", Supplier);
            ProjectsCompleted.Add("InvoiceNr", InvoiceNr);
            ProjectsCompleted.Add("EndDate", EndDate);



            Session["ProjectsCompleted"] = ProjectsCompleted;


            return Json(Url.Action("ProjectsCompleted", "Demands", new { @pageNr = "1" }));
        }
        //	data: '{ "PrjNo": "' + PrjNo + '", "OrderNr": "' + OrderNr + '", "Supplier": "' + Supplier + '","EndDate": "' + EndDate + '"}',

    }
}