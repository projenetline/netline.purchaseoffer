using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace netline.purchaseoffer.Models.BudgetModels
{
    public class Ntl_BudgetTransfer
    {
        public int Id { get; set; } = 0;
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; } = DateTime.Now;
        public double TotalAmountTo { get; set; } = 0;
        public double TotalAmountFrom { get; set; } = 0;
        public int TransferType { get; set; } = 0;
        public int TransferMonth { get; set; } = 0;
        public bool Transfered { get; set; } = false;
        public bool Locked { get; set; } = false;
        public bool Calculeted { get; set; } = false;
        public bool HasConfirmed { get; set; } = false;
        public bool HasRejected { get; set; } = false;
        public bool SendedConfirm { get; set; } = false;
        public List<Ntl_BudgetTransferLine> Lines { get; set; } = new List<Ntl_BudgetTransferLine>();
    }
    public class Ntl_BudgetTransferLine
    {
        public int Id { get; set; } = 0;
        public int LineNr { get; set; } = 0;
        public int TransferId { get; set; } = 0;
        public int TransferMonthFrom { get; set; } = 0;
        public int TransferMonthTo { get; set; } = 0;
        public string AccountFrom { get; set; } = string.Empty;
        public string AccountTo { get; set; } = string.Empty;
        public string AccountNameFrom { get; set; } = string.Empty;
        public string AccountNameTo { get; set; } = string.Empty;
        public string BranchFrom { get; set; } = string.Empty;
        public string BranchTo { get; set; } = string.Empty;
        public string CostCenterFrom { get; set; } = string.Empty;
        public string CostCenterTo { get; set; } = string.Empty;
        public string DepartmentFrom { get; set; } = string.Empty;
        public string DepartmentTo { get; set; } = string.Empty;
        public double AmountFrom { get; set; } = 0;
        public double AmountTo { get; set; } = 0;
        public double BudgetFrom { get; set; } = 0;
        public double BudgetTo { get; set; } = 0;
        public string AmountFromStr { get; set; } = string.Empty;
        public string AmountToStr { get; set; } = string.Empty;
        public string BudgetFromStr { get; set; } = string.Empty;
        public string BudgetToStr { get; set; } = string.Empty;

        public string NewBudgetFromStr { get; set; } = string.Empty;
        public string NewBudgetToStr { get; set; } = string.Empty;
        public string DemandPerson { get; set; } = string.Empty;
        public List<string> BudgetExpList { get; set; } = new List<string>();

        public string BudgetExp { get; set; } = string.Empty;
        public bool Confirmed { get; set; } = false;
        public bool Rejected { get; set; } = false;

        public bool Transfered { get; set; } = false;
        public List<SelectListItem> accountNameFromlist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> accountCodeFromlist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> BranchFromlist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CostCnterFromlist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DepartmentFromlist { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> accountNameTolist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> accountCodeTolist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> BranchTolist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CostCnterTolist { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DepartmentTolist { get; set; } = new List<SelectListItem>();

    }
    public class Ntl_BudgetTransferLineFrom
    {
        public int Id { get; set; } = 0;
        public int LineNr { get; set; } = 0;
        public int TransferId { get; set; } = 0;
        public int TransferMonthFrom { get; set; } = 0;

        public string AccountFrom { get; set; } = string.Empty;
        public string AccountNameFrom { get; set; } = string.Empty;
        public string BranchFrom { get; set; } = string.Empty;
        public string CostCenterFrom { get; set; } = string.Empty;
        public string DepartmentFrom { get; set; } = string.Empty;
        public double AmountFrom { get; set; } = 0;
        public double BudgetFrom { get; set; } = 0;
        public string AmountFromStr { get; set; } = string.Empty;
        public string BudgetFromStr { get; set; } = string.Empty;
        public string BudgetExp { get; set; } = string.Empty;
        public bool Confirmed { get; set; } = false;
        public string DemandPerson { get; set; } = string.Empty;
        public bool Rejected { get; set; } = false;

        public bool Transfered { get; set; } = false;
        
    }
    public class Ntl_BudgetTransferLineTo
    {
        public int Id { get; set; } = 0;
        public int LineNr { get; set; } = 0;
        public int TransferId { get; set; } = 0;
        public string AccountTo { get; set; } = string.Empty;
        public int TransferMonthTo { get; set; } = 0;
        public string AccountNameTo { get; set; } = string.Empty;
        public string BranchTo { get; set; } = string.Empty;
        public string CostCenterTo { get; set; } = string.Empty;
        public string DepartmentTo { get; set; } = string.Empty;
        public double AmountTo { get; set; } = 0;
        public double BudgetTo { get; set; } = 0;
        public string AmountToStr { get; set; } = string.Empty;
        public string BudgetToStr { get; set; } = string.Empty;
        public string BudgetExp { get; set; } = string.Empty;
        public bool Confirmed { get; set; } = false;
        public bool Rejected { get; set; } = false;
        public string DemandPerson { get; set; } = string.Empty;

        public bool Transfered { get; set; } = false;
    }
}


