namespace netline.purchaseoffer.Models
{
    public class Ntl_BudgetControl
    {
        public int[] TransRef { get; set; } = new int[] { };
        public double NetTotal { get; set; } = 0;
       
        public string BudgetCode { get; set; } = "";       
        public int LineId { get; set; } = 0;
        public Ntl_Budget Budget { get; set; } = new Ntl_Budget();
    }
    public class Ntl_Budget
    {
        public string BudgetCode { get; set; } = "";
        public int BudgetMonth { get; set; } = 0;
        public string BudgetName { get; set; } = "";
        public double Budget { get; set; } = 0;
        public double Gerceklesen { get; set; } = 0;
        public double BudgetYear { get; set; } = 0;
        public double GerceklesenYear { get; set; } = 0;
        public double BlokeAmount { get; set; } = 0;
        public double YillikBlokeAmount { get; set; } = 0;
        public bool BudgetYillik { get; set; } = false;
        public bool BudgetOk { get; set; } = true;
    }
    public class Ntl_BudgetResponse
    {
        public string BudgetInfo { get; set; } = "";
        public string BudgetCode { get; set; } = "";
        public bool BudgetOk { get; set; } = true;
    }
}
