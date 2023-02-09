using System;
using System.Collections.Generic;

namespace netline.purchaseoffer.Models
{
    public class jP_OfferSlip
    {
        public int internalReference { get; set; } = 0;
        public string slipNr { get; set; } = string.Empty;
        public DateTime slipDate { get; set; } = DateTime.Now;
        public int contrctrType { get; set; } = 0;
        public int status { get; set; } = 0;
        public int suffStatus { get; set; } = 0;
        public int offerType { get; set; } = 0;
        public int arpType { get; set; } = 0;
        public int groupType { get; set; } = 0;
        public DateTime slipStartDate { get; set; } = DateTime.Now;
        public DateTime slipEndDate { get; set; } = DateTime.Now;
        public jP_Arps arpRef=  new jP_Arps();
        public jP_OrgUnitRef orgUnitRef=  new jP_OrgUnitRef();
    

    }

    public class jP_OfferAlternatives
    {
        public jP_OfferSlipRef offerSlipRef { get; set; } = new jP_OfferSlipRef();
        public string AltNo { get; set; } = string.Empty;
        public int valid { get; set; } = 0;
        public jP_OfferTrans offerTrans { get; set; } = new jP_OfferTrans();
    }
    public class jP_OfferSlipRef
    {
        public int reference { get; set; }
        public int offerSlip_OfferType { get; set; } = 0;
        public string offerSlip_SlipNr { get; set; } = string.Empty;

    }
    public class jP_OfferTrans
    {
        public jP_Items[] items { get; set; } = new jP_Items[] { };

    }
    public class jP_Items
    {
        public jP_OfferSlipRef offerSlipRef { get; set; } = new jP_OfferSlipRef();
        public ReqRef reqRef { get; set; }       
        public int offerAltRef { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public double vat { get; set; }       
        public UOMRef uOMRef { get; set; }
         public int unitSetRef { get; set; }
        public double unitPrice { get; set; }
        
    }
    public class UOMRef
    {
        public int reference { get; set; }
        public int uomsetref { get; set; }
        
        public List<ExtraProp> extraProps { get; set; }
    }
    public class UnitSetRef
    {
        public int reference { get; set; }
        public List<ExtraProp> extraProps { get; set; }
    }
    public class ExtraProp
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class MasterReference
    {
        public int reference { get; set; }
        public List<ExtraProp> extraProps { get; set; }
    }
    public class ReqRef
    {
        public int reference { get; set; }
        public List<ExtraProp> extraProps { get; set; }
    }
}
