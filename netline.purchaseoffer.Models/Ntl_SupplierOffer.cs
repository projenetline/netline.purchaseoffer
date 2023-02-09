using System;
using System.Collections.Generic;

namespace netline.purchaseoffer.Models
{
    public class Ntl_SupplierOffer
    {
        public int ProjectId { get; set; } = 0;
        public int SupplierRef { get; set; } = 0;
        public string ProjectNr { get; set; } = string.Empty;
        public DateTime Date_ { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public TimeSpan EndTime { get; set; } = DateTime.Now.TimeOfDay;
        public string DeliveryAddress { get; set; } = string.Empty;
        public int Responded { get; set; } = 0;      
        public string Explanation { get; set; } = string.Empty;
        public string ResponseExplanation { get; set; } = string.Empty;
        public string NotResponseExplanation { get; set; } = string.Empty;        
        public string RequestGuid { get; set; } = "";
        public double UsdCurr { get; set; } = 0;
        public double EurCur { get; set; } = 0;

        public List<Ntl_OfferItems> Lines { get; set; } = new List<Ntl_OfferItems>();
        public List<Ntl_OfferDocs> files { get; set; } = new List<Ntl_OfferDocs>();
    }
    public class Ntl_OfferItems
    {
        public int Id { get; set; } = 0;
        public int ItemRef { get; set; } = 0;
        public int TrCurr { get; set; } = 0;
        public double TrRate { get; set; } = 0;
        public string TrCurrStr { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
        public string ItemGrpCode { get; set; } = string.Empty;
        public string SlipNr { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public double Quantity { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public double VatRate { get; set; } = 0;
        public double NewPriceWithVat { get; set; } = 0;
        public double NetTotalWithTax { get; set; } = 0;
        public double NewPrice { get; set; } = 0;
        public List<Ntl_SupplierOfferList> OfferList { get; set; } = new List<Ntl_SupplierOfferList>();
    }

    public class Ntl_SupplierOfferList
    {
        public int LineId { get; set; } = 0;
        public DateTime OfferTime { get; set; } = DateTime.Now;
        public string Explanation { get; set; } = string.Empty;
        public double Price { get; set; } = 0;
        public double Total { get; set; } = 0;
        public double VatRate { get; set; } = 0;
        public double NetTotal { get; set; } = 0;
    }

    public class Ntl_OfferRequest
    {
        public int Id { get; set; } = 0;
        public int SupplierRef { get; set; } = 0;
        public DateTime RequestDate_ { get; set; } = DateTime.Now;
        public int ProjectId { get; set; } = 0;
        public int RequestNr { get; set; } = 0;
        public int Responded { get; set; } = 0;
        public Guid RequestGuid { get; set; } = Guid.NewGuid();
      
    }
    public class Ntl_OfferDetail
    {
        public string ItemDesc { get; set; } = "";
        public double Quantity { get; set; } = 0;
        public string Unit { get; set; } = "";
        public string Department { get; set; } = "";
        public string Address { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string Sicil { get; set; } = "";
        public bool isDeliveryAddress { get; set; } = false;
        


    }
}
           