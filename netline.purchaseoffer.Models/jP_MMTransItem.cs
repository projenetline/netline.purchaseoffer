using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public class jP_MMTransItem
    {
        public int ItemRef { get; set; } = 0;
        public DateTime TransDate { get; set; } = DateTime.Today;
        public double NetTotal { get; set; } = 0;
    }
}
