namespace netline.purchaseoffer.Models
{
    public class jP_Global
    {
    }
    public class net_tokenResponse
    {
        public string success { get; set; } = string.Empty;
        public string serviceKey { get; set; } = string.Empty;
        public string authToken { get; set; } = string.Empty;
        public string authorization { get; set; } = string.Empty;
        public int service { get; set; } = 0;
    }
    public class jP_Arps
    {
        public int reference { get; set; } = 0;
        public string arp_Code { get; set; } = string.Empty;
    }
    public class jP_OrgUnitRef
    {
        public int reference { get; set; } = 0;
        public jP_OrgUnit_FirmRef orgUnit_FirmRef { get; set; } = new jP_OrgUnit_FirmRef();
        public string orgUnit_Code { get; set; } = string.Empty;
        public int orgUnit_DomainId { get; set; } = 0;
    }
    public class jP_OrgUnit_FirmRef
    {
        public int reference { get; set; } = 0;
        public int orgUnit_Firm_Companynr { get; set; } = 0;
        public int orgUnit_Firm_DomainId { get; set; } = 0;
    }

    public class jP_Response
    {
        public string apiVersion { get; set; } = string.Empty;
        public jP_Data data { get; set; } = new jP_Data();
    }
    public class jP_Data
    {
        public jP_Meta meta { get; set; } = new jP_Meta();
    }
    public class jP_Meta
    {
        public string href { get; set; } = string.Empty;
    }
}
