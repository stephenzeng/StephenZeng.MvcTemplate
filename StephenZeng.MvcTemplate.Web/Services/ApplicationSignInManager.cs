using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using StephenZeng.MvcTemplate.Common.Entities;

namespace StephenZeng.MvcTemplate.Web.Services
{
    public class ApplicationSignInManager : SignInManager<User, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : 
            base(userManager, authenticationManager) { }

        public override async Task<ClaimsIdentity> CreateUserIdentityAsync(User user)
        {
            var userManager = (ApplicationUserManager)UserManager;
            var claimsIdentity = await user.GenerateUserIdentityAsync(userManager);

            return claimsIdentity;
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}