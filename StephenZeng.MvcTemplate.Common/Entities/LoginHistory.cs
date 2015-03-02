using System;
using System.ComponentModel.DataAnnotations;

namespace StephenZeng.MvcTemplate.Common.Entities
{
    public class LoginHistory
    {
        public int Id { get; set; }

        [Display(Name = "Login email")]
        public string Email { get; set; }

        [Display(Name = "IP address")]
        public string Ip { get; set; }

        [Display(Name = "Attemp time")]
        public DateTime AttemptTime { get; set; }
        public LoginResult Result { get; set; }
    }

    public enum LoginResult
    {
        Success,
        [Display(Name = "Incorrect username or password")]
        IncorrectUsernameOrPassword,
        [Display(Name = "Email not confirmed")]
        EmailNotConfirmed,
        [Display(Name = "Not approved")]
        NotApproved,
        [Display(Name = "Locked out")]
        LockedOut,
        [Display(Name = "Requires verification")]
        RequiresVerification,
        Failed,
    }
}