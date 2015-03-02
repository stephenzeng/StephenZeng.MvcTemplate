using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using StephenZeng.MvcTemplate.Common.Entities;
using StephenZeng.MvcTemplate.Web.Helpers;
using StephenZeng.MvcTemplate.Web.Models;
using StephenZeng.MvcTemplate.Web.Services;

namespace StephenZeng.MvcTemplate.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IEmailService _emailService;

        public AccountController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindAsync(model.Email, model.Password);

            if (user == null)
            {
                await LogUserLoginAttemptAsync(model.Email, LoginResult.IncorrectUsernameOrPassword);
                ModelState.AddModelError("", "Incorrect email address or password.");
                return View(model);
            }

            if (!user.EmailConfirmed)
            {
                await LogUserLoginAttemptAsync(model.Email, LoginResult.EmailNotConfirmed);
                ModelState.AddModelError("", "Please check your email to confimr your registration first.");
                return View(model);
            }

            if (!user.Approved)
            {
                await LogUserLoginAttemptAsync(model.Email, LoginResult.NotApproved);
                ModelState.AddModelError("", "Your registration has been confirmed but not approved yet, as it is under the review and approval process. You will be notified by email once it is approved.");
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        await LogUserLoginAttemptAsync(model.Email, LoginResult.Success);
                        return RedirectToAction("Index", "Home");
                    }
                case SignInStatus.LockedOut:
                    {
                        await LogUserLoginAttemptAsync(model.Email, LoginResult.LockedOut);
                        return View("Lockout");
                    }
                case SignInStatus.RequiresVerification:
                    {
                        await LogUserLoginAttemptAsync(model.Email, LoginResult.RequiresVerification);
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
                    }
                case SignInStatus.Failure:
                default:
                    {
                        await LogUserLoginAttemptAsync(model.Email, LoginResult.Failed);
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                    }
            }
        }

        private async Task LogUserLoginAttemptAsync(string email, LoginResult result)
        {
            var loginAttempt = new LoginHistory()
            {
                Email = email,
                Ip = HttpContext.GetIp(),
                Result = result,
                AttemptTime = DateTime.Now,
            };

            DbContext.LoginHistories.Add(loginAttempt);
            Logger.Info(loginAttempt);
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl)
        {
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(await SignInManager.GetVerifiedUserIdAsync());

            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: false, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            Logger.Info("A user clicked register!");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Mapper.Map<User>(model);
                user.RegisterTime = DateTime.Now;
                user.LockoutEnabled = true;

                var alreadyExists = await UserManager.FindByEmailAsync(model.Email) != null;
                if (alreadyExists)
                {
                    ModelState.AddModelError("", "The email address has already been registered.");
                    return View(model);
                }
                
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var newUser = await UserManager.FindByNameAsync(model.Email);
                    var roleResult = await UserManager.AddToRoleAsync(newUser.Id, "User");

                    await SendRegistrationConfirmEmail(newUser);

                    return View("DisplayEmail");
                }
                AddErrors(result);
            }

            return View(model);
        }

        private async Task SendRegistrationConfirmEmail(User user, string ccAddress = null)
        {
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

            var emailContent = string.Format(@"<p>Dear {0} {1},</p>
                <p></p>
                <p>Thank you for your registration. In order to complete your registration, please click the link below, or copy it then open it in your browser.</p>
                <p>{2}</p>
                <p></p>
                <p></p>
                <p>This message was sent by system. Please do not reply.</p>
                <p>Mvc Template</p>", user.FirstName, user.LastName, callbackUrl);

            await _emailService.SendAsync("Mvc Template - Confirm your account", emailContent, user.Email, ccAddress);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
            {
                await SendComfirmNotification(userId);
            }

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        private async Task SendComfirmNotification(string userId)
        {
            var confirmedUser = await UserManager.FindByIdAsync(userId);
            var adminEmail = "";

            var roles = UserManager.GetRoles(userId);
            var emailContent = string.Format(@"<p>The following client has confirmed the registered email address:</p>
                <p>{0} {1}</p>
                <p>{2}</p>
                <p>Please click the following link to login the system to review and approve the client.</p>
                <p><a href=""{3}"">{3}</a></p>",
                confirmedUser.FirstName,
                confirmedUser.LastName,
                confirmedUser.Email,
                Url.Action("Login", "Account", new {}, protocol: Request.Url.Scheme));

            await _emailService.SendAsync("Complii Client Portfolio - Client confirmed email address", emailContent, adminEmail);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                ViewBag.Link = callbackUrl;
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            var flag = User.IsInRole("Admin");

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }


}