namespace netline.purchaseoffer.Models
{
    public class Ntl_Payplan
    {
        public int Id { get; set; } = 0;
        public int ProjectId { get; set; } = 0;
        public int LineNr { get; set; } = 0;
        public double Amount { get; set; } = 0;
        public string AmountStr { get; set; } = string.Empty;
    }
}
