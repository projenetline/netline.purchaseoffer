using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netline.purchaseoffer.Models
{
   public class Ntl_Order
    {
        public int slipType { get; set; }
        public string slipNumber { get; set; }
        public DateTime orderDate { get; set; }
        public double totalDiscounted { get; set; }
        public double totalVat { get; set; }
        public double totalGross { get; set; }
        public double totalNet { get; set; }
        public int deductionPart1 { get; set; }
        public int deductionPart2 { get; set; }
        public ArpRef arpRef { get; set; }
        public ArpOfReceipt arpOfReceipt { get; set; }
        public DepartmentRef departmentRef { get; set; }
        public DivisionRef divisionRef { get; set; }
        public SourceWHRef sourceWHRef { get; set; }
        public Transactions transactions { get; set; }
    }
    public class ArpRef
    {
        public int reference { get; set; }
        public string aRP_Code { get; set; }
    }

    public class ArpOfReceipt
    {
        public int reference { get; set; }
        public string aRPOfReceipt_Code { get; set; }
    }

    public class DepartmentFirmRef
    {
        public int reference { get; set; }
        public int department_Firm_Companynr { get; set; }
        public int department_Firm_DomainId { get; set; }
    }

    public class DepartmentRef
    {
        public int reference { get; set; }
        public DepartmentFirmRef department_FirmRef { get; set; }
        public string department_Code { get; set; }
        public int department_DomainId { get; set; }
    }

    public class DivisionFirmRef
    {
        public int reference { get; set; }
        public int division_Firm_Companynr { get; set; }
        public int division_Firm_DomainId { get; set; }
    }

    public class DivisionRef
    {
        public int reference { get; set; }
        public DivisionFirmRef division_FirmRef { get; set; }
        public string division_Code { get; set; }
        public int division_DomainId { get; set; }
    }

    public class SourceWHFirmRef
    {
        public int reference { get; set; }
        public int sourceWH_Firm_Companynr { get; set; }
        public int sourceWH_Firm_DomainId { get; set; }
    }

    public class SourceWHRef
    {
        public int reference { get; set; }
        public SourceWHFirmRef sourceWH_FirmRef { get; set; }
        public string sourceWH_Code { get; set; }
        public int sourceWH_DomainId { get; set; }
    }

    public class Order_ExtraProp
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Order_MasterReference
    {
        public int reference { get; set; }
        public List<Order_ExtraProp> extraProps { get; set; }
    }

    public class OrderSlipRef
    {
        public int reference { get; set; }
        public int orderSlip_SlipType { get; set; }
        public string orderSlip_SlipNumber { get; set; }
    }

    public class ARPRef2
    {
        public int reference { get; set; }
        public string aRP_Code { get; set; }
    }

    public class DepartmentFirmRef2
    {
        public int reference { get; set; }
        public int department_Firm_Companynr { get; set; }
        public int department_Firm_DomainId { get; set; }
    }

    public class DepartmentRef2
    {
        public int reference { get; set; }
        public DepartmentFirmRef2 department_FirmRef { get; set; }
        public string department_Code { get; set; }
        public int department_DomainId { get; set; }
    }

    public class DivisionFirmRef2
    {
        public int reference { get; set; }
        public int division_Firm_Companynr { get; set; }
        public int division_Firm_DomainId { get; set; }
    }

    public class DivisionRef2
    {
        public int reference { get; set; }
        public DivisionFirmRef2 division_FirmRef { get; set; }
        public string division_Code { get; set; }
        public int division_DomainId { get; set; }
    }

    public class SourceWHFirmRef2
    {
        public int reference { get; set; }
        public int sourceWH_Firm_Companynr { get; set; }
        public int sourceWH_Firm_DomainId { get; set; }
    }

    public class SourceWHRef2
    {
        public int reference { get; set; }
        public SourceWHFirmRef2 sourceWH_FirmRef { get; set; }
        public string sourceWH_Code { get; set; }
        public int sourceWH_DomainId { get; set; }
    }

    public class UOMUomsetref
    {
        public int reference { get; set; }
        public string uOM_Uomsetref_Code { get; set; }
    }

    public class Order_UOMRef
    {
        public int reference { get; set; }
        public string uOM_Code { get; set; }
        public UOMUomsetref uOM_Uomsetref { get; set; }
    }

    public class Item
    {
        public int detailLineId { get; set; }
        public int transType { get; set; }
        
        public int slipOrder { get; set; }
        public int slipType { get; set; }
        public DateTime slipDate { get; set; }
        public double quantity { get; set; }
        public double price { get; set; }
        public double total { get; set; }
        public double vATRate { get; set; }
        public double vATAmount { get; set; }
        public double vATBase { get; set; }
        public int unitSetRef { get; set; }
        public int uOMMultiplier { get; set; }
        public int uOMDivisor { get; set; }
        public DateTime dueDate { get; set; }
        public double netTotal { get; set; }
        public Order_MasterReference master_Reference { get; set; }
        public OrderSlipRef orderSlipRef { get; set; }
        public ARPRef2 aRPRef { get; set; }
        public DepartmentRef2 departmentRef { get; set; }
        public DivisionRef2 divisionRef { get; set; }
        public SourceWHRef2 sourceWHRef { get; set; }
        public Order_UOMRef uOMRef { get; set; }
        public ADDetailLines aDDetailLines { get; set; }
    }

    public class Transactions
    {
        public List<Item> items { get; set; }
    }
    public class SlipRef
    {
        public int reference { get; set; }
        public int slip_SlipType { get; set; }
        public string slip_SlipNumber { get; set; }
    }

    public class AnalysisDimRef
    {
        public int reference { get; set; }
        public string analysisDim_Code { get; set; }
    }

    public class OUFirmRef
    {
        public int reference { get; set; }
        public int oU_Firm_Companynr { get; set; }
        public int oU_Firm_DomainId { get; set; }
    }

    public class OURef
    {
        public int reference { get; set; }
        public OUFirmRef oU_FirmRef { get; set; }
        public string oU_Code { get; set; }
        public int oU_DomainId { get; set; }
    }

    public class Item2
    {
       
        public int transRef { get; set; }
        public int lineNr { get; set; }
        public int distributionRatio { get; set; }
        public int lCNet { get; set; }
        public DateTime slipDate { get; set; }
        public int uOMRef { get; set; }
        public int unitSetRef { get; set; }
        public int uOMMultiplier { get; set; }
        public int uOMDivisor { get; set; }
        public double quantity { get; set; }
        public int slipType { get; set; }
        public int sourceWHRef { get; set; }
        public SlipRef slipRef { get; set; }
        public AnalysisDimRef analysisDimRef { get; set; }
        public OURef oURef { get; set; }
    }

    public class ADDetailLines
    {
        public List<Item2> items { get; set; }
    }

   

   

}
