using System.ComponentModel.DataAnnotations;

namespace StephenZeng.MvcTemplate.Web.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Role name")]
        public string Name { get; set; }
    }
}