using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RIT.HMS.Domain.MasterEnums
{
    public class Ennums
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }
    public enum EnumCustomerStatus
    {
        VVIP,
        VIP,
        Other       
    }
    public enum Gender
    {
        Male,
        Female
        
    }
    public enum CivilStatus
    {
        [Display(Name = "Married")]
        Married=1,
        [Display(Name = "Un Married")]
        Unmarried=2

    }

    public enum ColorsEnum : int
    {
        SuperAdmin = 0,  
        PhoenixAdmin = 1,  
        OfficeAdmin = 2,  
        ReportUser = 3,  
        BillingUser = 4  
    }

    public enum SpecialDay
    {
        [Display(Name = "Sinhal Hindu New Year")]
        NewYear=1,
        [Display(Name = "Thai Pongal")]
        ThaiPongal=2,
        [Display(Name = "Vesak")]
        Vesak=3,
        [Display(Name = "Ramazan")]
        Ramazan=4,
        [Display(Name = "Haj Festival")]
        HajFestival=5,
        [Display(Name = "Xmas")]
        Xmas=6
    }


    public enum enumJobStatus
    {
        Ongoing = 0,
        Completed = 1,
      
       
    }

}