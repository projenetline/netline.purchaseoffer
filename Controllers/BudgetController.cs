using netline.purchaseoffer.BusinessLayer;
using netline.purchaseoffer.Models;
using netline.purchaseoffer.Models.BudgetModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace netline.purchaseoffer.Controllers
{
    public class BudgetController : Controller
    {
        // GET: Budget

        ProjectUtil util = new ProjectUtil();
        [SessionsController]
        public ActionResult Index()
        {

            List<Ntl_BudgetTransfer> budgetTransfers= util.getBudgetTransfers();
            return View(budgetTransfers);
        }
        [SessionsController]
        public ActionResult NewTransfer()
        {





            List<SelectListItem> accountNamelist = new List<SelectListItem>();
            List<SelectListItem> accountCodelist = new List<SelectListItem>();
            foreach (var item in util.getBudgetAccounts())
            {
                accountNamelist.Add(new SelectListItem { Text = item.name, Value = item.value });
                accountCodelist.Add(new SelectListItem { Text = item.value, Value = item.name });
            }

            List<SelectListItem> Branchlist = new List<SelectListItem>();
            List<SelectListItem> CostCnterlist = new List<SelectListItem>();
            foreach (var item in util.getBudgetCosts())
            {
                Branchlist.Add(new SelectListItem { Text = item.name, Value = item.value });
                CostCnterlist.Add(new SelectListItem { Text = item.value, Value = item.name });
            }


            List<SelectListItem> Departmentlist = new List<SelectListItem>();

            Departmentlist.Add(new SelectListItem { Text = "Finans", Value = "Finans" });
            Departmentlist.Add(new SelectListItem { Text = "IT", Value = "IT" });
            Departmentlist.Add(new SelectListItem { Text = "İnsan Kaynakları", Value = "İnsan Kaynakları" });
            Departmentlist.Add(new SelectListItem { Text = "Strateji İç iletişim", Value = "Strateji İç iletişim" });
            Departmentlist.Add(new SelectListItem { Text = "Pazarlama", Value = "Pazarlama" });
            Departmentlist.Add(new SelectListItem { Text = "İş Zekası", Value = "İş Zekası" });
            Departmentlist.Add(new SelectListItem { Text = "İdari İşler", Value = "İdari İşler" });

            List<SelectListItem> transferTypelist = new List<SelectListItem>();
            transferTypelist.Add(new SelectListItem { Text = "Yıllık", Value = "1" });
            transferTypelist.Add(new SelectListItem { Text = "Aylık", Value = "2" });


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

            Ntl_BudgetTransfer budgetTransfer= new Ntl_BudgetTransfer();
            Ntl_BudgetTransferLine line=new Ntl_BudgetTransferLine();
            budgetTransfer.TotalAmountTo = 0;
            budgetTransfer.TotalAmountFrom = 0;




            for (int i = 0; i < 15; i++)
            {
                line = new Ntl_BudgetTransferLine();
                line.LineNr = i + 1;

                line.DepartmentFromlist = Departmentlist;
                line.accountNameFromlist = accountNamelist;
                line.accountCodeFromlist = accountCodelist;
                line.BranchFromlist = Branchlist;
                line.CostCnterFromlist = CostCnterlist;
                line.TransferMonthFrom = DateTime.Today.Month;

                line.DepartmentTolist = Departmentlist;
                line.accountNameTolist = accountNamelist;
                line.accountCodeTolist = accountCodelist;
                line.BranchTolist = Branchlist;
                line.CostCnterTolist = CostCnterlist;
                line.TransferMonthTo = DateTime.Today.Month;
                budgetTransfer.Lines.Add(line);
            }

            ViewBag.transferMonthlist = transferMonthlist;
            ViewBag.transferTypelist = transferTypelist;
            budgetTransfer.TransferDate = DateTime.Today;
            budgetTransfer.TransferNo = util.getBudgetTransferNo();
            budgetTransfer.TransferMonth = DateTime.Today.Month;
            budgetTransfer.TransferType = 1;

            return View(budgetTransfer);
        }
        [HttpPost]
        [SessionsController]
        public ActionResult NewTransfer(Ntl_BudgetTransfer budgetTransfer, string command)
        {
            List<Ntl_BudgetTransferLineFrom> linesFrom = new List<Ntl_BudgetTransferLineFrom>();
            List<Ntl_BudgetTransferLineTo> linesTo = new List<Ntl_BudgetTransferLineTo>();
            foreach (var line in budgetTransfer.Lines)
            {
                budgetTransfer.TotalAmountFrom = 0;
                budgetTransfer.TotalAmountTo = 0;
                line.AmountFrom = Convert.ToDouble(line.AmountFromStr);
                line.AmountTo = Convert.ToDouble(line.AmountToStr);
                line.BudgetFrom = Convert.ToDouble(line.BudgetFromStr);
                line.BudgetTo = Convert.ToDouble(line.BudgetToStr);
                if (line.AmountFrom > 0)
                {
                    Ntl_BudgetTransferLineFrom lineFrom = new Ntl_BudgetTransferLineFrom()
                    {
                        AccountFrom=line.AccountFrom,
                        AccountNameFrom =  line.AccountNameFrom,
                        AmountFromStr = line.AmountFromStr,
                        TransferMonthFrom=line.TransferMonthFrom,
                        LineNr=line.LineNr,
                        AmountFrom=line.AmountFrom,
                        BranchFrom=line.BranchFrom,
                        BudgetFrom=line.BudgetFrom,
                        BudgetFromStr=line.BudgetFromStr,
                        CostCenterFrom=line.CostCenterFrom,
                        DepartmentFrom=line.DepartmentFrom,
                        Id=line.Id,
                        TransferId=line.TransferId,
                        DemandPerson=line.DemandPerson
                    };
                    linesFrom.Add(lineFrom);
                }
                if (line.AmountTo > 0)
                {
                    Ntl_BudgetTransferLineTo lineTo=new Ntl_BudgetTransferLineTo()
                    {
                        TransferId=line.TransferId,
                        Id=line.Id,
                        AccountNameTo=line.AccountNameTo,
                        TransferMonthTo=line.TransferMonthTo,
                        AccountTo=line.AccountTo,
                        AmountTo=line.AmountTo,
                        AmountToStr=line.AmountToStr,
                        BranchTo=line.BranchTo,
                        BudgetTo=line.BudgetTo,
                        BudgetToStr=line.BudgetToStr,
                        CostCenterTo=line.CostCenterTo,
                        DepartmentTo=line.DepartmentTo,
                        LineNr=line.LineNr,
                        DemandPerson=line.DemandPerson
                    };
                    linesTo.Add(lineTo);
                }
            }

            List<Ntl_BudgetTransferLine> BudgetLines= new List<Ntl_BudgetTransferLine>();
            Ntl_BudgetTransferLine BudgetLine= new Ntl_BudgetTransferLine();
            int addLineNr=1;
            int lineCount=0;
            if (linesFrom.Count > linesTo.Count)
                lineCount = linesFrom.Count;
            else
                lineCount = linesTo.Count;
            for (int i = 0; i < 30; i++)
            {
                if (i < linesFrom.Count && i < linesTo.Count)
                {
                    if (linesFrom[i].AmountFrom > linesTo[i].AmountTo)
                    {


                        Ntl_BudgetTransferLineFrom lineFrom = new Ntl_BudgetTransferLineFrom()
                        {
                            AccountFrom=linesFrom[i].AccountFrom,
                            AccountNameFrom =  linesFrom[i].AccountNameFrom,
                            TransferMonthFrom = linesFrom[i].TransferMonthFrom,
                            AmountFromStr = linesFrom[i].AmountFromStr,
                            LineNr=linesFrom[i].LineNr,
                            AmountFrom=linesFrom[i].AmountFrom - linesTo[i].AmountTo,
                            BranchFrom=linesFrom[i].BranchFrom,
                            BudgetFrom=linesFrom[i].BudgetFrom,
                            BudgetFromStr=linesFrom[i].BudgetFromStr,
                            CostCenterFrom=linesFrom[i].CostCenterFrom,
                            DepartmentFrom=linesFrom[i].DepartmentFrom,
                            Id=linesFrom[i].Id,
                            TransferId=linesFrom[i].TransferId,
                            DemandPerson=linesFrom[i].DemandPerson
                        };
                        linesFrom[i].AmountFrom = linesTo[i].AmountTo;
                        linesFrom.Insert(addLineNr, lineFrom);

                    }
                    else if (linesFrom[i].AmountFrom < linesTo[i].AmountTo)
                    {


                        Ntl_BudgetTransferLineTo lineTo=new Ntl_BudgetTransferLineTo()
                        {
                            TransferId=linesTo[i].TransferId,
                            Id=linesTo[i].Id,
                            TransferMonthTo=linesTo[i].TransferMonthTo,
                            AccountNameTo=linesTo[i].AccountNameTo,
                            AccountTo=linesTo[i].AccountTo,
                            AmountTo=linesTo[i].AmountTo-linesFrom[i].AmountFrom,
                            AmountToStr=linesTo[i].AmountToStr,
                            BranchTo=linesTo[i].BranchTo,
                            BudgetTo=linesTo[i].BudgetTo,
                            BudgetToStr=linesTo[i].BudgetToStr,
                            CostCenterTo=linesTo[i].CostCenterTo,
                            DepartmentTo=linesTo[i].DepartmentTo,
                            LineNr=linesTo[i].LineNr,
                            DemandPerson=linesTo[i].DemandPerson
                        };
                        linesTo[i].AmountTo = linesFrom[i].AmountFrom;
                        linesTo.Insert(addLineNr, lineTo);

                    }
                    else
                    {




                    }
                    addLineNr++;
                }
                else
                {


                }

            }




            for (int i = 0; i < 15; i++)
            {
                BudgetLine = new Ntl_BudgetTransferLine();
                if (i < linesFrom.Count)
                {
                    BudgetLine.AccountFrom = linesFrom[i].AccountFrom;
                    BudgetLine.AccountNameFrom = linesFrom[i].AccountNameFrom;
                    BudgetLine.TransferMonthFrom = linesFrom[i].TransferMonthFrom;
                    BudgetLine.AmountFrom = linesFrom[i].AmountFrom;
                    BudgetLine.AmountFromStr = linesFrom[i].AmountFrom.ToString("n2");
                    BudgetLine.BranchFrom = linesFrom[i].BranchFrom;
                    BudgetLine.BudgetFrom = linesFrom[i].BudgetFrom;
                    BudgetLine.BudgetFromStr = linesFrom[i].BudgetFrom.ToString("n2");
                    BudgetLine.CostCenterFrom = linesFrom[i].CostCenterFrom;
                    BudgetLine.DepartmentFrom = linesFrom[i].DepartmentFrom;
                    BudgetLine.TransferId = linesFrom[i].TransferId;
                    BudgetLine.DemandPerson = linesFrom[i].DemandPerson;
                    budgetTransfer.TotalAmountFrom += +linesFrom[i].AmountFrom;

                }
                if (i < linesTo.Count)
                {
                    BudgetLine.AccountNameTo = linesTo[i].AccountNameTo;
                    BudgetLine.TransferMonthTo = linesTo[i].TransferMonthTo;
                    BudgetLine.AccountTo = linesTo[i].AccountTo;
                    BudgetLine.AmountTo = linesTo[i].AmountTo;
                    BudgetLine.AmountToStr = linesTo[i].AmountTo.ToString("n2");
                    BudgetLine.BranchTo = linesTo[i].BranchTo;
                    BudgetLine.BudgetTo = linesTo[i].BudgetTo;
                    BudgetLine.BudgetToStr = linesTo[i].BudgetTo.ToString("n2");
                    BudgetLine.CostCenterTo = linesTo[i].CostCenterTo;
                    BudgetLine.DepartmentTo = linesTo[i].DepartmentTo;
                    BudgetLine.TransferId = linesTo[i].TransferId;
                    BudgetLine.DemandPerson = linesTo[i].DemandPerson;
                    budgetTransfer.TotalAmountTo += linesTo[i].AmountTo;
                }
                BudgetLine.LineNr = i + 1;
                BudgetLines.Add(BudgetLine);
            }



            if (command == "CalculateBudget")
            {
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


                List<SelectListItem> transferTypelist = new List<SelectListItem>();
                transferTypelist.Add(new SelectListItem { Text = "Yıllık", Value = "1" });
                transferTypelist.Add(new SelectListItem { Text = "Aylık", Value = "2" });


                ViewBag.transferMonthlist = transferMonthlist;
                ViewBag.transferTypelist = transferTypelist;

                List<Ntl_SelectValue> BudgetAccounts = util.getBudgetAccounts();
                List<Ntl_SelectValue> BudgetCosts = util.getBudgetCosts();
                List<Ntl_SelectValue> Departmentlist = new List<Ntl_SelectValue>();
                Departmentlist.Add(new Ntl_SelectValue { name = "Finans", value = "Finans" });
                Departmentlist.Add(new Ntl_SelectValue { name = "IT", value = "IT" });
                Departmentlist.Add(new Ntl_SelectValue { name = "İnsan Kaynakları", value = "İnsan Kaynakları" });
                Departmentlist.Add(new Ntl_SelectValue { name = "Strateji İç iletişim", value = "Strateji İç iletişim" });
                Departmentlist.Add(new Ntl_SelectValue { name = "Pazarlama", value = "Pazarlama" });
                Departmentlist.Add(new Ntl_SelectValue { name = "İş Zekası", value = "İş Zekası" });
                Departmentlist.Add(new Ntl_SelectValue { name = "İdari İşler", value = "İdari İşler" });

                foreach (var BudgetLine_ in BudgetLines)
                {



                    BudgetLine_.accountNameFromlist = new List<SelectListItem>();
                    BudgetLine_.accountCodeFromlist = new List<SelectListItem>();
                    foreach (var BudgetAccount in BudgetAccounts)
                    {
                        if (BudgetAccount.name == BudgetLine_.AccountNameFrom)
                        {
                            BudgetLine_.accountNameFromlist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = true });
                            BudgetLine_.accountCodeFromlist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = true });
                        }
                        else
                        {
                            BudgetLine_.accountNameFromlist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = false });
                            BudgetLine_.accountCodeFromlist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = false });
                        }
                    }
                    BudgetLine_.CostCnterFromlist = new List<SelectListItem>();
                    BudgetLine_.BranchFromlist = new List<SelectListItem>();

                    foreach (var BudgetCost in BudgetCosts)
                    {
                        if (BudgetCost.value.ToString() == BudgetLine_.BranchFrom)
                        {
                            BudgetLine_.CostCnterFromlist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = true });
                            BudgetLine_.BranchFromlist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = true });
                        }
                        else
                        {
                            BudgetLine_.CostCnterFromlist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = false });
                            BudgetLine_.BranchFromlist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = false });
                        }
                    }



                    BudgetLine_.DepartmentFromlist = new List<SelectListItem>();
                    foreach (var dep in Departmentlist)
                    {
                        if (dep.name == BudgetLine_.DepartmentFrom)
                        {
                            BudgetLine_.DepartmentFromlist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = true });

                        }
                        else
                        {
                            BudgetLine_.DepartmentFromlist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = false });
                        }
                    }





                    BudgetLine_.accountNameTolist = new List<SelectListItem>();
                    BudgetLine_.accountCodeTolist = new List<SelectListItem>();
                    foreach (var BudgetAccount in BudgetAccounts)
                    {
                        if (BudgetAccount.name == BudgetLine_.AccountNameTo)
                        {
                            BudgetLine_.accountNameTolist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = true });
                            BudgetLine_.accountCodeTolist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = true });
                        }
                        else
                        {
                            BudgetLine_.accountNameTolist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = false });
                            BudgetLine_.accountCodeTolist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = false });
                        }
                    }
                    BudgetLine_.CostCnterTolist = new List<SelectListItem>();
                    BudgetLine_.BranchTolist = new List<SelectListItem>();

                    foreach (var BudgetCost in BudgetCosts)
                    {
                        if (BudgetCost.value.ToString() == BudgetLine_.BranchTo)
                        {
                            BudgetLine_.CostCnterTolist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = true });
                            BudgetLine_.BranchTolist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = true });
                        }
                        else
                        {
                            BudgetLine_.CostCnterTolist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = false });
                            BudgetLine_.BranchTolist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = false });
                        }
                    }



                    BudgetLine_.DepartmentTolist = new List<SelectListItem>();
                    foreach (var dep in Departmentlist)
                    {
                        if (dep.name == BudgetLine_.DepartmentTo)
                        {
                            BudgetLine_.DepartmentTolist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = true });

                        }
                        else
                        {
                            BudgetLine_.DepartmentTolist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = false });
                        }
                    }


                }
                budgetTransfer.Lines = BudgetLines;


                TempData["BudgetTransfer"] = budgetTransfer;
                return RedirectToAction("CalculatedBudget", "Budget");
            }
            else if (command == "SaveBudget")
            {

                if (budgetTransfer.TotalAmountTo == budgetTransfer.TotalAmountFrom)
                {

                    int Id=  util.saveBudgetTransfer(budgetTransfer);
                    TempData["BudgetTransfer"] = budgetTransfer;
                    return RedirectToAction("Transfer", "Budget", new { Id = Id });
                }
                else
                {

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


                    List<SelectListItem> transferTypelist = new List<SelectListItem>();
                    transferTypelist.Add(new SelectListItem { Text = "Yıllık", Value = "1" });
                    transferTypelist.Add(new SelectListItem { Text = "Aylık", Value = "2" });


                    ViewBag.transferMonthlist = transferMonthlist;
                    ViewBag.transferTypelist = transferTypelist;

                    List<Ntl_SelectValue> BudgetAccounts = util.getBudgetAccounts();
                    List<Ntl_SelectValue> BudgetCosts = util.getBudgetCosts();
                    List<Ntl_SelectValue> Departmentlist = new List<Ntl_SelectValue>();
                    Departmentlist.Add(new Ntl_SelectValue { name = "Finans", value = "Finans" });
                    Departmentlist.Add(new Ntl_SelectValue { name = "IT", value = "IT" });
                    Departmentlist.Add(new Ntl_SelectValue { name = "İnsan Kaynakları", value = "İnsan Kaynakları" });
                    Departmentlist.Add(new Ntl_SelectValue { name = "Strateji İç iletişim", value = "Strateji İç iletişim" });
                    Departmentlist.Add(new Ntl_SelectValue { name = "Pazarlama", value = "Pazarlama" });
                    Departmentlist.Add(new Ntl_SelectValue { name = "İş Zekası", value = "İş Zekası" });
                    Departmentlist.Add(new Ntl_SelectValue { name = "İdari İşler", value = "İdari İşler" });

                    foreach (var BudgetLine_ in BudgetLines)
                    {



                        BudgetLine_.accountNameFromlist = new List<SelectListItem>();
                        BudgetLine_.accountCodeFromlist = new List<SelectListItem>();
                        foreach (var BudgetAccount in BudgetAccounts)
                        {
                            if (BudgetAccount.name == BudgetLine_.AccountNameFrom)
                            {
                                BudgetLine_.accountNameFromlist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = true });
                                BudgetLine_.accountCodeFromlist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = true });
                            }
                            else
                            {
                                BudgetLine_.accountNameFromlist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = false });
                                BudgetLine_.accountCodeFromlist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = false });
                            }
                        }
                        BudgetLine_.CostCnterFromlist = new List<SelectListItem>();
                        BudgetLine_.BranchFromlist = new List<SelectListItem>();

                        foreach (var BudgetCost in BudgetCosts)
                        {
                            if (BudgetCost.name == BudgetLine_.BranchFrom)
                            {
                                BudgetLine_.CostCnterFromlist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = true });
                                BudgetLine_.BranchFromlist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = true });
                            }
                            else
                            {
                                BudgetLine_.CostCnterFromlist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = false });
                                BudgetLine_.BranchFromlist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = false });
                            }
                        }



                        BudgetLine_.DepartmentFromlist = new List<SelectListItem>();
                        foreach (var dep in Departmentlist)
                        {
                            if (dep.name == BudgetLine_.DepartmentFrom)
                            {
                                BudgetLine_.DepartmentFromlist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = true });

                            }
                            else
                            {
                                BudgetLine_.DepartmentFromlist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = false });
                            }
                        }





                        BudgetLine_.accountNameTolist = new List<SelectListItem>();
                        BudgetLine_.accountCodeTolist = new List<SelectListItem>();
                        foreach (var BudgetAccount in BudgetAccounts)
                        {
                            if (BudgetAccount.name == BudgetLine_.AccountNameTo)
                            {
                                BudgetLine_.accountNameTolist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = true });
                                BudgetLine_.accountCodeTolist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = true });
                            }
                            else
                            {
                                BudgetLine_.accountNameTolist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = false });
                                BudgetLine_.accountCodeTolist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = false });
                            }
                        }
                        BudgetLine_.CostCnterTolist = new List<SelectListItem>();
                        BudgetLine_.BranchTolist = new List<SelectListItem>();

                        foreach (var BudgetCost in BudgetCosts)
                        {
                            if (BudgetCost.name == BudgetLine_.BranchTo)
                            {
                                BudgetLine_.CostCnterTolist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = true });
                                BudgetLine_.BranchTolist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = true });
                            }
                            else
                            {
                                BudgetLine_.CostCnterTolist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = false });
                                BudgetLine_.BranchTolist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = false });
                            }
                        }



                        BudgetLine_.DepartmentTolist = new List<SelectListItem>();
                        foreach (var dep in Departmentlist)
                        {
                            if (dep.name == BudgetLine_.DepartmentTo)
                            {
                                BudgetLine_.DepartmentTolist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = true });

                            }
                            else
                            {
                                BudgetLine_.DepartmentTolist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = false });
                            }
                        }


                    }
                    budgetTransfer.Lines = BudgetLines;


                    TempData["BudgetTransfer"] = budgetTransfer;
                    return RedirectToAction("CalculatedBudget", "Budget");


                }

            }














            return View(budgetTransfer);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult GetValue(string Code, string TransferType, string TransferMonth)
        {
            double value=util.getBudgetValue(Code,TransferType,TransferMonth);



            return Json(value.ToString("n2"), JsonRequestBehavior.AllowGet);


        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult GetValueStr(string value)
        {
            double value_Dbl=0;
            double.TryParse(value, out value_Dbl);
            return Json(value_Dbl.ToString("n2"), JsonRequestBehavior.AllowGet);


        }
        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult GetDoubleToStr(double value)
        {

            return Json(value.ToString("n2"), JsonRequestBehavior.AllowGet);


        }
        [SessionsController]
        public ActionResult CalculatedBudget()
        {
            Ntl_BudgetTransfer budgetTransfer  =(Ntl_BudgetTransfer)TempData["BudgetTransfer"];


            List<SelectListItem> transferTypelist = new List<SelectListItem>();
            transferTypelist.Add(new SelectListItem { Text = "Yıllık", Value = "1" });
            transferTypelist.Add(new SelectListItem { Text = "Aylık", Value = "2" });


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
            ViewBag.transferTypelist = transferTypelist;

            return View(budgetTransfer);
        }


        [SessionsController]
        public ActionResult Transfer(int Id)
        {

            Ntl_BudgetTransfer budgetTransfer= util.getBudgetTransfer(Id);
            budgetTransfer.TotalAmountTo = 0;
            budgetTransfer.TotalAmountFrom = 0;
            List<Ntl_BudgetTransferLineFrom> linesFrom = new List<Ntl_BudgetTransferLineFrom>();
            List<Ntl_BudgetTransferLineTo> linesTo = new List<Ntl_BudgetTransferLineTo>();
            foreach (var line in budgetTransfer.Lines)
            {

                if (line.AmountFrom > 0)
                {
                    Ntl_BudgetTransferLineFrom lineFrom = new Ntl_BudgetTransferLineFrom()
                    {
                        AccountFrom=line.AccountFrom,
                        AccountNameFrom =  line.AccountNameFrom,
                        TransferMonthFrom=line.TransferMonthFrom,
                        AmountFromStr = line.AmountFromStr,
                        LineNr=line.LineNr,
                        AmountFrom=line.AmountFrom,
                        BranchFrom=line.BranchFrom,
                        BudgetFrom=line.BudgetFrom,
                        BudgetFromStr=line.BudgetFromStr,
                        CostCenterFrom=line.CostCenterFrom,
                        DepartmentFrom=line.DepartmentFrom,
                        Id=line.Id,
                        TransferId=line.TransferId,
                        BudgetExp=line.BudgetExp,
                        Rejected=line.Rejected,
                        Confirmed=true
                        ,Transfered=line.Transfered
                    };
                    linesFrom.Add(lineFrom);
                }
                if (line.AmountTo > 0)
                {
                    Ntl_BudgetTransferLineTo lineTo=new Ntl_BudgetTransferLineTo()
                    {
                        TransferId=line.TransferId,
                        Id=line.Id,
                        AccountNameTo=line.AccountNameTo,

                        TransferMonthTo=line.TransferMonthTo,
                        AccountTo=line.AccountTo,
                        AmountTo=line.AmountTo,
                        AmountToStr=line.AmountToStr,
                        BranchTo=line.BranchTo,
                        BudgetTo=line.BudgetTo,
                        BudgetToStr=line.BudgetToStr,
                        CostCenterTo=line.CostCenterTo,
                        DepartmentTo=line.DepartmentTo,
                        LineNr=line.LineNr,
                        BudgetExp=line.BudgetExp,
                        Rejected=line.Rejected,
                        DemandPerson=line.DemandPerson,
                        Confirmed=true,
                        Transfered=line.Transfered
                    };
                    linesTo.Add(lineTo);
                }
            }

            List<Ntl_BudgetTransferLine> BudgetLines= new List<Ntl_BudgetTransferLine>();
            Ntl_BudgetTransferLine BudgetLine= new Ntl_BudgetTransferLine();
            int addLineNr=1;
            int lineCount=0;
            if (linesFrom.Count > linesTo.Count)
                lineCount = linesFrom.Count;
            else
                lineCount = linesTo.Count;
            for (int i = 0; i < 30; i++)
            {
                if (i < linesFrom.Count && i < linesTo.Count)
                {
                    if (linesFrom[i].AmountFrom > linesTo[i].AmountTo)
                    {


                        Ntl_BudgetTransferLineFrom lineFrom = new Ntl_BudgetTransferLineFrom()
                        {
                            AccountFrom=linesFrom[i].AccountFrom,
                            AccountNameFrom =  linesFrom[i].AccountNameFrom,
                            TransferMonthFrom=linesFrom[i].TransferMonthFrom,
                            AmountFromStr = linesFrom[i].AmountFromStr,
                            LineNr=linesFrom[i].LineNr,
                            AmountFrom=linesFrom[i].AmountFrom - linesTo[i].AmountTo,
                            BranchFrom=linesFrom[i].BranchFrom,
                            BudgetFrom=linesFrom[i].BudgetFrom,
                            BudgetFromStr=linesFrom[i].BudgetFromStr,
                            CostCenterFrom=linesFrom[i].CostCenterFrom,
                            DepartmentFrom=linesFrom[i].DepartmentFrom,
                            Id=linesFrom[i].Id,
                            TransferId=linesFrom[i].TransferId,
                            BudgetExp=linesFrom[i].BudgetExp,
                            Rejected=linesFrom[i].Rejected,
                            DemandPerson=linesFrom[i].DemandPerson,
                            Confirmed=true,
                            Transfered=linesFrom[i].Transfered

                        };
                        linesFrom[i].AmountFrom = linesTo[i].AmountTo;
                        linesFrom.Insert(addLineNr, lineFrom);

                    }
                    else if (linesFrom[i].AmountFrom < linesTo[i].AmountTo)
                    {


                        Ntl_BudgetTransferLineTo lineTo=new Ntl_BudgetTransferLineTo()
                        {
                            TransferId=linesTo[i].TransferId,
                            Id=linesTo[i].Id,
                            AccountNameTo=linesTo[i].AccountNameTo,
                            TransferMonthTo=linesTo[i].TransferMonthTo,
                            AccountTo=linesTo[i].AccountTo,
                            AmountTo=linesTo[i].AmountTo-linesFrom[i].AmountFrom,
                            AmountToStr=linesTo[i].AmountToStr,
                            BranchTo=linesTo[i].BranchTo,
                            BudgetTo=linesTo[i].BudgetTo,
                            BudgetToStr=linesTo[i].BudgetToStr,
                            CostCenterTo=linesTo[i].CostCenterTo,
                            DepartmentTo=linesTo[i].DepartmentTo,
                            LineNr=linesTo[i].LineNr,
                            BudgetExp=linesTo[i].BudgetExp,
                            Rejected=linesTo[i].Rejected,
                            DemandPerson=linesTo[i].DemandPerson,
                            Confirmed=true,
                            Transfered=linesTo[i].Transfered

                        };
                        linesTo[i].AmountTo = linesFrom[i].AmountFrom;
                        linesTo.Insert(addLineNr, lineTo);

                    }
                    else
                    {




                    }
                    addLineNr++;
                }
                else
                {


                }

            }




            for (int i = 0; i < 15; i++)
            {
                BudgetLine = new Ntl_BudgetTransferLine();
                if (i < linesFrom.Count)
                {
                    BudgetLine.AccountFrom = linesFrom[i].AccountFrom;
                    BudgetLine.TransferMonthFrom = linesFrom[i].TransferMonthFrom;
                    BudgetLine.AccountNameFrom = linesFrom[i].AccountNameFrom;
                    BudgetLine.AmountFrom = linesFrom[i].AmountFrom;
                    BudgetLine.AmountFromStr = linesFrom[i].AmountFrom.ToString("n2");
                    BudgetLine.BranchFrom = linesFrom[i].BranchFrom;
                    BudgetLine.BudgetFrom = linesFrom[i].BudgetFrom;
                    BudgetLine.BudgetFromStr = linesFrom[i].BudgetFrom.ToString("n2");
                    BudgetLine.CostCenterFrom = linesFrom[i].CostCenterFrom;
                    BudgetLine.DepartmentFrom = linesFrom[i].DepartmentFrom;
                    BudgetLine.TransferId = linesFrom[i].TransferId;
                    BudgetLine.BudgetExp = linesFrom[i].BudgetExp;
                    BudgetLine.Rejected = linesFrom[i].Rejected;
                    BudgetLine.DemandPerson = linesFrom[i].DemandPerson;
                    budgetTransfer.TotalAmountFrom += +linesFrom[i].AmountFrom;
                    BudgetLine.Id = linesFrom[i].Id;
                    BudgetLine.Confirmed = true;
                    BudgetLine.Transfered = linesFrom[i].Transfered;
                }
                if (i < linesTo.Count)
                {
                    BudgetLine.AccountNameTo = linesTo[i].AccountNameTo;
                    BudgetLine.TransferMonthTo = linesTo[i].TransferMonthTo;
                    BudgetLine.AccountTo = linesTo[i].AccountTo;
                    BudgetLine.AmountTo = linesTo[i].AmountTo;
                    BudgetLine.AmountToStr = linesTo[i].AmountTo.ToString("n2");
                    BudgetLine.BranchTo = linesTo[i].BranchTo;
                    BudgetLine.BudgetTo = linesTo[i].BudgetTo;
                    BudgetLine.BudgetToStr = linesTo[i].BudgetTo.ToString("n2");
                    BudgetLine.CostCenterTo = linesTo[i].CostCenterTo;
                    BudgetLine.DepartmentTo = linesTo[i].DepartmentTo;
                    BudgetLine.TransferId = linesTo[i].TransferId;
                    BudgetLine.BudgetExp = linesTo[i].BudgetExp;
                    BudgetLine.Rejected = linesTo[i].Rejected;
                    BudgetLine.DemandPerson = linesTo[i].DemandPerson;
                    budgetTransfer.TotalAmountTo += linesTo[i].AmountTo;
                    BudgetLine.Confirmed = true;
                    BudgetLine.Id = linesTo[i].Id;
                    BudgetLine.Transfered = linesTo[i].Transfered;
                }
                BudgetLine.LineNr = i + 1;
                BudgetLines.Add(BudgetLine);
            }

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


            List<SelectListItem> transferTypelist = new List<SelectListItem>();
            transferTypelist.Add(new SelectListItem { Text = "Yıllık", Value = "1" });
            transferTypelist.Add(new SelectListItem { Text = "Aylık", Value = "2" });


            ViewBag.transferMonthlist = transferMonthlist;
            ViewBag.transferTypelist = transferTypelist;

            List<Ntl_SelectValue> BudgetAccounts = util.getBudgetAccounts();
            List<Ntl_SelectValue> BudgetCosts = util.getBudgetCosts();
            List<Ntl_SelectValue> Departmentlist = new List<Ntl_SelectValue>();
            Departmentlist.Add(new Ntl_SelectValue { name = "Finans", value = "Finans" });
            Departmentlist.Add(new Ntl_SelectValue { name = "IT", value = "IT" });
            Departmentlist.Add(new Ntl_SelectValue { name = "İnsan Kaynakları", value = "İnsan Kaynakları" });
            Departmentlist.Add(new Ntl_SelectValue { name = "Strateji İç iletişim", value = "Strateji İç iletişim" });
            Departmentlist.Add(new Ntl_SelectValue { name = "Pazarlama", value = "Pazarlama" });
            Departmentlist.Add(new Ntl_SelectValue { name = "İş Zekası", value = "İş Zekası" });
            Departmentlist.Add(new Ntl_SelectValue { name = "İdari İşler", value = "İdari İşler" });

            foreach (var BudgetLine_ in BudgetLines)
            {



                BudgetLine_.accountNameFromlist = new List<SelectListItem>();
                BudgetLine_.accountCodeFromlist = new List<SelectListItem>();
                foreach (var BudgetAccount in BudgetAccounts)
                {
                    if (BudgetAccount.name == BudgetLine_.AccountNameFrom || BudgetAccount.value == BudgetLine_.AccountFrom)
                    {
                        BudgetLine_.accountNameFromlist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = true });
                        BudgetLine_.accountCodeFromlist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = true });
                        BudgetLine_.AccountNameFrom = BudgetAccount.name;
                        BudgetLine_.AccountFrom = BudgetAccount.value;
                    }
                    else
                    {
                        BudgetLine_.accountNameFromlist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = false });
                        BudgetLine_.accountCodeFromlist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = false });
                    }
                }
                BudgetLine_.CostCnterFromlist = new List<SelectListItem>();
                BudgetLine_.BranchFromlist = new List<SelectListItem>();

                foreach (var BudgetCost in BudgetCosts)
                {
                    if (BudgetCost.name == BudgetLine_.BranchFrom)
                    {
                        BudgetLine_.CostCnterFromlist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = true });
                        BudgetLine_.BranchFromlist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = true });
                    }
                    else
                    {
                        BudgetLine_.CostCnterFromlist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = false });
                        BudgetLine_.BranchFromlist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = false });
                    }
                }



                BudgetLine_.DepartmentFromlist = new List<SelectListItem>();
                foreach (var dep in Departmentlist)
                {
                    if (dep.name == BudgetLine_.DepartmentFrom)
                    {
                        BudgetLine_.DepartmentFromlist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = true });

                    }
                    else
                    {
                        BudgetLine_.DepartmentFromlist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = false });
                    }
                }





                BudgetLine_.accountNameTolist = new List<SelectListItem>();
                BudgetLine_.accountCodeTolist = new List<SelectListItem>();
                foreach (var BudgetAccount in BudgetAccounts)
                {
                    if (BudgetAccount.name == BudgetLine_.AccountNameTo || BudgetAccount.value == BudgetLine_.AccountTo)
                    {
                        BudgetLine_.accountNameTolist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = true });
                        BudgetLine_.accountCodeTolist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = true });
                        BudgetLine_.AccountTo = BudgetAccount.value;
                        BudgetLine_.AccountNameTo = BudgetAccount.name;
                    }
                    else
                    {
                        BudgetLine_.accountNameTolist.Add(new SelectListItem { Text = BudgetAccount.name, Value = BudgetAccount.value, Selected = false });
                        BudgetLine_.accountCodeTolist.Add(new SelectListItem { Text = BudgetAccount.value, Value = BudgetAccount.name, Selected = false });
                    }
                }
                BudgetLine_.CostCnterTolist = new List<SelectListItem>();
                BudgetLine_.BranchTolist = new List<SelectListItem>();

                foreach (var BudgetCost in BudgetCosts)
                {
                    if (BudgetCost.name == BudgetLine_.BranchTo)
                    {
                        BudgetLine_.CostCnterTolist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = true });
                        BudgetLine_.BranchTolist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = true });
                    }
                    else
                    {
                        BudgetLine_.CostCnterTolist.Add(new SelectListItem { Text = BudgetCost.name, Value = BudgetCost.value, Selected = false });
                        BudgetLine_.BranchTolist.Add(new SelectListItem { Text = BudgetCost.value, Value = BudgetCost.name, Selected = false });
                    }
                }



                BudgetLine_.DepartmentTolist = new List<SelectListItem>();
                foreach (var dep in Departmentlist)
                {
                    if (dep.name == BudgetLine_.DepartmentTo)
                    {
                        BudgetLine_.DepartmentTolist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = true });

                    }
                    else
                    {
                        BudgetLine_.DepartmentTolist.Add(new SelectListItem { Text = dep.name, Value = dep.value, Selected = false });
                    }
                }


            }
            budgetTransfer.Lines = BudgetLines;
            budgetTransfer.HasConfirmed = true;
            budgetTransfer.SendedConfirm = true;
            return View(budgetTransfer);
        }
        [SessionsController]
        public ActionResult DoTransfer(int Id)
        {
            var Line= util.getBudgetDetail(Id);

            int TransferType=   util.getBudgetTransferType(Line.TransferId);
            int TransferMonth=   util.getBudgetTransferMonth(Line.TransferId);

            if (TransferType == 1)
            {




                int totalMonth=(12-TransferMonth)+1;
                double AmountFrom =Line.AmountFrom / totalMonth;
                double AmountTo =Line.AmountTo / totalMonth;

                for (int i = TransferMonth; i < 13; i++)
                {
                    util.updateAmountFromBudget(Line.AccountFrom, i.ToString(), AmountFrom);
                    util.updateAmountToBudget(Line.AccountTo, i.ToString(), AmountTo);
                }


            }
            else
            {
                util.updateAmountFromBudget(Line.AccountFrom, Line.TransferMonthFrom.ToString(), Line.AmountFrom);
                util.updateAmountToBudget(Line.AccountTo, Line.TransferMonthTo.ToString(), Line.AmountTo);


            }

            util.updateBudgetTransfered(Id, Line.TransferId);
            return RedirectToAction("Index");
        }

        [SessionsController]
        public ActionResult SendToConfirm(int Id)
        {
            util.saveBudgetConfirm(Id, 0);



            return RedirectToAction("Index");
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

        public ActionResult Confirm(string ConfirmId)
        {

            int ProjectId = util.getConfirmProjectId(ConfirmId);

            Ntl_Request  offer = util.getResponses(ProjectId);

            return View(offer);
        }
        [SessionsController]
        public ActionResult ProjectsStatus(string pageNr)
        {
            Dictionary<string, string> ProjectsCompleted= new Dictionary<string, string>();
            ProjectsCompleted.Add("PrjNo", "");
            ProjectsCompleted.Add("OrderNr", "");
            ProjectsCompleted.Add("Supplier", "");
            ProjectsCompleted.Add("InvoiceNr", "");
            ProjectsCompleted.Add("EndDate", "");
            if (Session["ProjectOnOrder"] != null)
            {
                ProjectsCompleted = (Dictionary<string, string>)Session["ProjectsCompleted"];
            }

            List<Ntl_BrowserOffer> offerList=util.getProjectsCompleted(Convert.ToInt16(pageNr),ProjectsCompleted);
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
            return View(offerList);


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
        public ActionResult getBudgetDetail(int budgetLineId)
        {
            var budgetDetail= util.getBudgetDetail(budgetLineId);
            budgetDetail.AmountFromStr = budgetDetail.AmountFrom.ToString("N2");
            budgetDetail.AmountToStr = budgetDetail.AmountTo.ToString("N2");
            budgetDetail.BudgetFromStr = budgetDetail.BudgetFrom.ToString("N2");
            budgetDetail.BudgetToStr = budgetDetail.BudgetTo.ToString("N2");

            budgetDetail.NewBudgetFromStr = (budgetDetail.BudgetFrom - budgetDetail.AmountFrom).ToString("N2");
            budgetDetail.NewBudgetToStr = (budgetDetail.BudgetTo + budgetDetail.AmountTo).ToString("N2");

            return Json(budgetDetail, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult updateBudgetTransferConfirm(int budgetLineId, int status, string exp)
        {




            Ntl_User user =(Ntl_User)Session["User"];

            if (status == -1)
            {
                util.updateBudgetConfirm(budgetLineId, status, exp, user.Email);
            }
            else
            {
                util.updateBudgetConfirm(budgetLineId, status, exp, user.Email);

                var budgetDetail= util.getBudgetDetail(budgetLineId);

                util.saveBudgetDetailConfirm(budgetDetail.TransferId, user.Email);

            }


            return Json("", JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult getRejectDetail(int budgetLineId)
        {
            var budgetDetail= util.getRejectDetail(budgetLineId);


            return Json(budgetDetail, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ActionResult sendToConfirm(int BudgetLineId, string RejectResponse)
        {
            try
            {
                util.updateBudgetConfirmResponse(BudgetLineId, RejectResponse);
                util.saveBudgetDetailConfirm(BudgetLineId, 0);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }





        }

    }
}