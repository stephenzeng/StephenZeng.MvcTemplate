using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace StephenZeng.MvcTemplate.Common.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public string Address { get; set; }

        [Display(Name = "Register time")]
        public DateTime RegisterTime { get; set; }

        public string Ip { get; set; }

        public bool Approved { get; set; }

        public bool Locked { get; set; }

        public bool Deleted { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            userIdentity.AddClaim(new Claim(ClaimTypes.GivenName, FirstName));
            userIdentity.AddClaim(new Claim(ClaimTypes.Surname, LastName));

            return userIdentity;
        }
    }
}
