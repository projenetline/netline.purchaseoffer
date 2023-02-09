using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public  class Ntl_Comment
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public string PersonName { get; set; } = string.Empty;
        public string PersonEmail { get; set; } = string.Empty;
        public byte[] Comment { get; set; } = new byte[] { };
        public string CommentStr { get; set; } = string.Empty;
        public Guid CommentGuid { get; set; } = Guid.NewGuid();
        public DateTime CommentTime { get; set; } = DateTime.Now;
        public bool Status_ { get; set; } = false;
    }
  


  
}


