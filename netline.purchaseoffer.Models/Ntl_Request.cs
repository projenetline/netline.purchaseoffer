using System.Collections.Generic;

namespace netline.purchaseoffer.Models
{
    public class Ntl_Request
    {
        public int ProjectId { get; set; } = 0;
        public bool WaitingConfirm { get; set; } = true;        
        public List<Ntl_Budget> Budgets { get; set; } = new List<Ntl_Budget>();
        public List<Ntl_RequestSupplier> RequestSuppliers { get; set; } = new List<Ntl_RequestSupplier>();
    }
    public class Ntl_RequestSupplier
    {

        public int Responded { get; set; } = 0;
        public string RespondMessage { get; set; } = "";
        public double NetTotal { get; set; } = 0;
        public bool BudgetControl { get; set; } = true;
        public int SupplierRef { get; set; } = 0;
        public int RequestNr { get; set; } = 0;
        public Ntl_OfferSupplier Supplier { get; set; } = new Ntl_OfferSupplier();
        public List<Ntl_RequestSupplierLine> supplierLines { get; set; } = new List<Ntl_RequestSupplierLine>();
    }
    public class Ntl_RequestSupplierLine
    {
        public int RequestNr { get; set; } = 0;
        public string BudgetCode { get; set; } = "";
        public double Total { get; set; } = 0;
    }

}
