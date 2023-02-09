namespace netline.purchaseoffer.Models
{
    public class Ntl_User
    {
        public int Id { get; set; } = 0;
        public string UserName { get; set; } = "";
        public string ActiveName { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";        
        public string Email { get; set; } = "";
        public int UserType { get; set; } = 0;
        public string FullName { get; set; } = "";
        public string AnlyCode { get; set; } = "";        
        public bool Budget { get; set; } =false;
        public bool RequirementDemand { get; set; } = false;
        public bool ResetPassword { get; set; } = false;
    }
    
}
