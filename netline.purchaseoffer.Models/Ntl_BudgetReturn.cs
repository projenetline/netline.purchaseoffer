using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public  class Ntl_BudgetReturn
    {
        public double NetTotal { get; set; } = 0;
        public string BudgetCode { get; set; } = "";
        public string DemandNr { get; set; } = "";
        public bool isContract { get; set; } = false;
    }
}
