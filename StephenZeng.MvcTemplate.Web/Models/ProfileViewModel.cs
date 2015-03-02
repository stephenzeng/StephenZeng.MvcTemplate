using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StephenZeng.MvcTemplate.Web.Models
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [Display(Name = "Registered at")]
        public DateTime RegisterTime { get; set; }
    }

    public class ClientProfileViewModel : ProfileViewModel
    {
        public string AdviserEmail { get; set; }
        public IEnumerable<string> Accounts { get; set; }
    }
}