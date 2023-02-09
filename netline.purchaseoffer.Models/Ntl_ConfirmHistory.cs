using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public class Ntl_ConfirmHistory
    {

        public string Person { get; set; } = "";
        public string ConfirmType { get; set; } = "";
        public string Duration { get; set; } = "";      
        public DateTime Time_ { get; set; } = DateTime.Now;
        public string TimeStr { get; set; } = "";
        public string DateStr { get; set; } = "";
        public DateTime BeforeTime_ { get; set; } = DateTime.Now;
    }
}
