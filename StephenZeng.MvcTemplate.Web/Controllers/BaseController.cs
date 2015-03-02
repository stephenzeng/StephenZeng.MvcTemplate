using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using StephenZeng.MvcTemplate.Web.Dal;
using StephenZeng.MvcTemplate.Web.Helpers;
using StephenZeng.MvcTemplate.Web.Services;

namespace StephenZeng.MvcTemplate.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private ApplicationDbContext _dbContext;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationSignInManager _signInManager;

        private IEnumerable<string> _accountList;
        private readonly int _pageSize;

        protected BaseController()
        {
            Logger = LogManager.GetLogger(GetType().FullName);
            _pageSize = int.Parse(ConfigurationManager.AppSettings["PageSize"]);
        }

        public int PageSize { get { return _pageSize; } }

        protected Logger Logger { get; private set; }

        public ApplicationDbContext DbContext
        {
            get
            {
                if (_dbContext == null) _dbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                return _dbContext;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null) _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                return _userManager;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                if (_roleManager == null) _roleManager = HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
                return _roleManager;
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                if (_signInManager == null) _signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
                return _signInManager;
            }
        }

        protected void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected IEnumerable<string> CurrentUserAccounts
        {
            get
            {
                if (_accountList == null)
                    _accountList = User.Identity.GetClaimValue("AccountNumber").Split(',');

                return _accountList;
            }
        }
    }
}