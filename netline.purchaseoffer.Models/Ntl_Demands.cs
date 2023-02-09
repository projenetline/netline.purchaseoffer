using System;
using System.Collections.Generic;

namespace netline.purchaseoffer.Models
{

    public class Ntl_Demands
    {
        public List<Ntl_Demand> Demands { get; set; } = new List<Ntl_Demand>();        
    }

    public class Ntl_Demand
    {
        public bool Select { get; set; } = false;
        public int SlipRef { get; set; } = 0;
        public int TransRef { get; set; } = 0;
        public string SlipNr { get; set; } = string.Empty;
        public string DemandNr { get; set; } = string.Empty;
        public DateTime SlipDate { get; set; } = DateTime.Now;
        public string SlipDateStr { get; set; } = string.Empty;
        public string ItemGrpCode { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string PersonEmail { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string LineExp { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
        public double StockAmount { get; set; } = 0;
        public double Quantity { get; set; } = 0;
        public double LineNet { get; set; } = 0;
        

    }
    public class Ntl_DemandFilter
    {
       
       
        public string SlipNr { get; set; } = string.Empty;
        public string SlipBegDate { get; set; } = string.Empty;
        public string SlipEndDate { get; set; } = string.Empty;
        public DateTime Begdate { get; set; }
        public DateTime Enddate { get; set; }
        public string[] ItemGrpCode { get; set; } = new string[] { };
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
        public string Person { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;     
        public string Usage { get; set; } = string.Empty;
 

    }

}
