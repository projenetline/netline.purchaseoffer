using System;

namespace netline.purchaseoffer.Models
{
    public class Ntl_Confirm
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public int ConfirmNr { get; set; } = 0;
        public DateTime Date_ { get; set; } = DateTime.Now;
        public string ProjectNo { get; set; } = string.Empty; 
        public string PersonName { get; set; } = string.Empty;
        public string PersonEmail { get; set; } = string.Empty;
        public string ProjectStatus{ get; set; } = string.Empty;
        public byte[] Comment { get; set; } = new byte[] { };
        public string CommentStr { get; set; } = string.Empty;
        public Guid ConfirmGuid { get; set; } = Guid.NewGuid();
        public DateTime ConfirmTime { get; set; } = DateTime.Now;
        public int ConfirmStatus { get; set; } = 0;
        public int ConfirmType { get; set; } = 0;
        public string Supplier { get; set; } = string.Empty;
        
    }


    public class Ntl_BudgetConfirm
    {
        public int Id { get; set; } = 0;   
        

        public string PersonName { get; set; } = string.Empty;
        public string PersonEmail { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;
        public string CommentResponse { get; set; } = string.Empty;
      
        public DateTime ConfirmTime { get; set; } = DateTime.Now;


    }

    public class Ntl_InvoiceConfirm
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;  
        public string ProjectNo { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string PersonEmail { get; set; } = string.Empty;
        public byte[] Comment { get; set; } = new byte[] { };
        public string CommentStr { get; set; } = string.Empty;
        public Guid ConfirmGuid { get; set; } = Guid.NewGuid();
        public DateTime ConfirmTime { get; set; } = DateTime.Now;
        public int ConfirmStatus { get; set; } = 0;
        public string Explanation { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;

    }

}
