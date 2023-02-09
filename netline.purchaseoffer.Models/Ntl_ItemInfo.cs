namespace netline.purchaseoffer.Models
{
    public class Ntl_ItemInfo
    {
        public int ItemRef { get; set; } = 0;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public int UomRef { get; set; } = 0;
        public int UomSetRef { get; set; } = 0;
        public string UnitSetCode { get; set; } = string.Empty;
    }
}
