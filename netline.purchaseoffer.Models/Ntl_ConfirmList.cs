using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public class Ntl_ConfirmList
    {
        public int Id { get; set; } = 0;
        public int ConfirmNr { get; set; } = 0;    
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double ConfirmLimit { get; set; } = 0;
        public bool Budget { get; set; } = false;
    }
}
