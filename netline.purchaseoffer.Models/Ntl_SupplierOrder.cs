using System;
using System.Collections.Generic;
using System.Web;

namespace netline.purchaseoffer.Models
{
    public class Ntl_SupplierOrder
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public string ProjectNr { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.Today;
        public int SupplierRef { get; set; } = 0;
        public string SupplierCode { get; set; } = string.Empty;
        public string SupplierDesc { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public List<Ntl_OrderDocs> DocList { get; set; } = new List<Ntl_OrderDocs>();
        public HttpPostedFileBase Uploadfile { get; set; }
        public List<Ntl_SupplierOrderLine> Lines { get; set; } = new List<Ntl_SupplierOrderLine>();
        public double NetTotal { get; set; } = 0;
        public Guid RequestGuid { get; set; } = Guid.Empty;
        public bool SendedSupplier { get; set; } = false;
        public bool IsSend { get; set; } = false;
        public string Supplier_EMail { get; set; } = string.Empty;
        public string CC_EMail1 { get; set; } = string.Empty;
        public string CC_EMail2 { get; set; } = string.Empty;
        public string CC_EMail3 { get; set; } = string.Empty;
    }

    public class Ntl_SupplierOrderLine
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public int OrderId { get; set; } = 0;
        public int ItemRef { get; set; } = 0;
        public string ItemDesc { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string TrCurr { get; set; } = string.Empty;
        public double Price { get; set; } = 0;
        public double TrRate { get; set; } = 0;
        public double Quantity { get; set; } = 0;
        public double VatRate { get; set; } = 0;
        public double NetTotal { get; set; } = 0;
        public Guid RequestGuid { get; set; } = Guid.Empty;
        public string Address { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;       
    }
    public class Ntl_OrderDocsUploadResult
    {
        public string Result { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public List<Ntl_OrderDocs> Docs { get; set; } = new List<Ntl_OrderDocs>();
    }

    public class Ntl_OrderDocs
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public string UploadedFilePath { get; set; } = string.Empty;
        public string UploadedFileName { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string UploadedFileContentTyp { get; set; } = string.Empty;
    }
}

