using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public class Ntl_ProjectStatus
    {
        public string CommentPerson { get; set; } = string.Empty;
        public string ConfirmPerson { get; set; } = string.Empty;
        public string RejectPerson { get; set; } = string.Empty;

        public string ProjectStatus { get; set; } = string.Empty;

        public int SuggestionSupplierRef { get; set; } = 0;
        public int WaitingConfirm { get; set; } = 0;
        public int ConfirmedPersonel { get; set; } = 0;
        public int CommentCount{ get; set; } = 0;
        public double PaymentPlan { get; set; } = 0;
        

    }
}
