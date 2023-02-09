using System;

namespace netline.purchaseoffer.Models
{


    public class Ntl_BrowserOffer
    {
        public int ProjectId { get; set; } = 0;
        public int SuggestionSupplierRef { get; set; } = 0;
        public string ProjectNr { get; set; } = "";
        public DateTime EndDate { get; set; } = DateTime.Now;
        public TimeSpan EndTime { get; set; } = DateTime.Now.TimeOfDay;
        public DateTime DueDate { get; set; } = DateTime.Now;
        public Ntl_OfferSupplier Supplier { get; set; } = new Ntl_OfferSupplier();
        public int OrderSlipRef { get; set; } = 0;
        public int InvoiceSlipRef { get; set; } = 0;
        public string Status_ { get; set; } = "";
        public int ProjectStatus { get; set; } = 0;
        public bool InvoiceControl { get; set; } = false;
        public bool InvoiceConfirmed { get; set; } = false;
        public int SendedConfirm { get; set; } = 0;
        public double Difference { get; set; } = 0;
        public string Explanation { get; set; } = "";

        public Ntl_InvoiceInfo InvInfo { get; set; } = new Ntl_InvoiceInfo();
        public Ntl_OrderInfo OrderInfo { get; set; } = new Ntl_OrderInfo();

    }
    public class Ntl_InvoiceInfo
    {
        public int InvoiceSlipRef { get; set; } = 0;
        public int InvoiceStatus{ get; set; } = 0;
        public string InvoiceNr { get; set; } = "";
        public double InvoiceNetTotal { get; set; } = 0;
    }
    public class Ntl_OrderInfo
    {
        public int OrderSlipRef { get; set; } = 0;
        public string OrderNr { get; set; } = "";
        public double OrderNetTotal { get; set; } = 0;
    }
}