using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using StephenZeng.MvcTemplate.Common.Entities;
using StephenZeng.MvcTemplate.Web.Services;

namespace StephenZeng.MvcTemplate.Web.Dal
{
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        public ApplicationDbInitializer()
        {
            SqlConnection.ClearAllPools();
        }

        private static ApplicationUserManager UserManager
        {
            get { return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        private static ApplicationRoleManager RoleManager
        {
            get { return HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>(); }
        }

        protected override void Seed(ApplicationDbContext context)
        {
            InitializeRoles();
            InitializeAdmin();

            base.Seed(context);
        }

        private void InitializeRoles()
        {
            var roles = new[] { "Admin", "User" };

            foreach (var roleName in roles)
            {
                var role = RoleManager.FindByName(roleName);
                if (role == null)
                {
                    role = new IdentityRole(roleName);
                    var roleresult = RoleManager.Create(role);
                }
            }
        }

        private void InitializeAdmin()
        {
            var roleName = "Admin";
            var email = "test@gmail.com";
            var password = "1qaz@WSX";

            var role = RoleManager.FindByName(roleName);
            var user = UserManager.FindByName(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = "System",
                    LastName = "Admin",
                    PhoneNumber = "08 9999 0000",
                    EmailConfirmed = true,
                    Approved = true,
                    RegisterTime = DateTime.Now
                };
                var result = UserManager.Create(user, password);
                result = UserManager.SetLockoutEnabled(user.Id, false);
                UserManager.AddToRole(user.Id, roleName);
            }
        }
    }
}