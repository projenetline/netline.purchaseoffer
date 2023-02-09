using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
    public class jP_OrderItem
    {
        public int ItemRef { get; set; } = 0;
        public string BudgetCode { get; set; } = string.Empty;
        public double NetTotal { get; set; } = 0;
    }
}
