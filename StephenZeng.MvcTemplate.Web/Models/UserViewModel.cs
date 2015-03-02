using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StephenZeng.MvcTemplate.Web.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public IEnumerable<string> Roles { get; set; }
        
        [Display(Name = "Email confirmed")]
        public bool EmailConfirmed { get; set; }

        public bool Approved { get; set; }

        [Display(Name = "Register time")]
        public DateTime RegisterTime { get; set; }
    }
}