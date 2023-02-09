using System;

namespace netline.purchaseoffer.Models
{
    public class Ntl_Filter
    {
        public DateTime BegDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string Supplier { get; set; } = "";
    }
}
