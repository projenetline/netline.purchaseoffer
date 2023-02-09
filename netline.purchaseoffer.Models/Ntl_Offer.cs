using System;
using System.Collections.Generic;
using System.Web;

namespace netline.purchaseoffer.Models
{
    public class Ntl_Offer
    {
        public int ProjectId { get; set; } = 0;
        public int OfferCount { get; set; } = 0;
        public int OfferNr { get; set; } = 0;
        public int SuggestionSupplierRef { get; set; } = 0;
        public string SuggestionExplanation { get; set; } = string.Empty;
        public string ProjectNr { get; set; } = string.Empty;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public DateTime OfferDate { get; set; } = DateTime.Now;        
        public TimeSpan EndTime { get; set; } = DateTime.Now.TimeOfDay;
        public string DeliveryAddress { get; set; } = string.Empty;
        public bool isDeliveryAddress { get; set; } = false;       
        public string Explanation { get; set; } = string.Empty;
        public List<Ntl_OfferLine> Lines { get; set; } = new List<Ntl_OfferLine>();
        public List<Ntl_OfferSupplier> Suppliers { get; set; } = new List<Ntl_OfferSupplier>();
        public List<Ntl_OfferDocs> DocList { get; set; } = new List<Ntl_OfferDocs>();
        public HttpPostedFileBase uploadfile { get; set; }
        public List<Ntl_Comment> Comments { get; set; } = new List<Ntl_Comment>();



    }
    public class Ntl_OfferForOrder
    {
        public int ProjectId { get; set; } = 0;      
        public int OfferNr { get; set; } = 0;
        public string ProjectNr { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public List<Ntl_OfferLine> Lines { get; set; } = new List<Ntl_OfferLine>();
        public Ntl_OfferSupplier Supplier { get; set; } = new Ntl_OfferSupplier();
    }


    public class Ntl_OfferLine
    {
        public int Id { get; set; } = 0;
        public int transType { get; set; } = 0;
        public int TransRef_ { get; set; } = 0;
        public int ItemRef { get; set; } = 0;
        public int TrCurr { get; set; } = 0;
        public double TrRate { get; set; } = 0;
        public string TrCurrStr { get; set; } = string.Empty;
        public List<int> TransRef { get; set; } = new List<int>();
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string ItemGrpCode { get; set; } = string.Empty;
        public string SlipNr { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public double Quantity { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
        public double LastPurchPrice { get; set; } = 0;
        public double Total { get; set; } = 0;
        public double VatRate { get; set; } = 0;
        public double NetTotal { get; set; } = 0;
        public bool isContract { get; set; } = false;
        public List<Ntl_SupplierOfferPrice> Priceses { get; set; } = new List<Ntl_SupplierOfferPrice>();
    }
    public class Ntl_OfferSupplier
    {
        public int Id { get; set; } = 0;
        public int SupplierRef { get; set; } = 0;
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierDesc { get; set; } = string.Empty;
        public string TaxNr { get; set; } = string.Empty;
        public bool selectSupplier { get; set; } = false;
        public bool isControlled { get; set; } = false;
        public List<Ntl_NetTotal> NetTotals { get; set; } = new List<Ntl_NetTotal>();


    }

    public class Ntl_OfferDocsUploadResult
    {
        public string Result { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public List<Ntl_OfferDocs> Docs { get; set; } = new List<Ntl_OfferDocs>();
    }
    public class Ntl_OfferDocs
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public string UploadedFilePath { get; set; } = string.Empty;
        public string UploadedFileName { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string UploadedFileContentTyp { get; set; } = string.Empty;
    }

    public class Ntl_SupplierOfferPrice
    {
        public double Price { get; set; } = 0;
        public string Explanation { get; set; } = string.Empty;
        public int RequestNr { get; set; } = 0;
        public int TrCurr { get; set; } = 0;
        public int VatRate { get; set; } = 0;
        public double TrRate { get; set; } = 0;
        public double TrNet { get; set; } = 0;
        public string TrCurrStr { get; set; } = string.Empty;

    }
    public class Ntl_NetTotal
    {
        public double NetTotal { get; set; } = 0;
        public double NetTotalWithoutTax { get; set; } = 0;
        public double TrCurr { get; set; } = 0;
        public double VatRate { get; set; } = 0;
        public int RequestNr { get; set; } = 0;
        public bool OfferSend { get; set; } = true;
        public bool Closed { get; set; } = true;
        public bool RequestSend { get; set; } = true;
        public bool Lowest { get; set; } = true;
        public int Responded { get; set; } = 1;
        public Guid RequestGuid { get; set; } = Guid.Empty;
        
    }
}
