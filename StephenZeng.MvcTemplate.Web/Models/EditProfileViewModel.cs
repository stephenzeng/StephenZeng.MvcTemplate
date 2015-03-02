using System.ComponentModel.DataAnnotations;

namespace StephenZeng.MvcTemplate.Web.Models
{
    public class EditProfileViewModel
    {
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }
    }
}