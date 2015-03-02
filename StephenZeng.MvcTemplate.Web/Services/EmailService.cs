using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SendGrid;

namespace StephenZeng.MvcTemplate.Web.Services
{
    public interface IEmailService : IIdentityMessageService, IDisposable
    {
        Task SendAsync(string subject, string body, string toAddress, string ccAddress = null, string bccAddress = null);
    }

    public class EmailService : IEmailService
    {
        private readonly string _accountName;
        private readonly string _password;
        private readonly string _systemEmail;

        public EmailService(string accountName, string password, string systemEmail)
        {
            _accountName = accountName;
            _password = password;
            _systemEmail = systemEmail;
        }

        public Task SendAsync(IdentityMessage message)
        {
            var email = ConfigEmail(message);
            return Send(email);
        }

        public Task SendAsync(string subject, string body, string toAddress, string ccAddress = null, string bccAddress = null)
        {
            var email = new SendGridMessage();
            email.AddTo(toAddress);
            email.Subject = subject;
            email.Text = body;
            email.Html = body;

            if (!string.IsNullOrEmpty(ccAddress))
                email.AddCc(new MailAddress(ccAddress));

            if (!string.IsNullOrEmpty(bccAddress))
                email.AddBcc(new MailAddress(bccAddress));

            return Send(email);
        }

        private Task Send(SendGridMessage email)
        {
            email.From = new MailAddress(_systemEmail);

            var credentials = new NetworkCredential(_accountName, _password);
            var transportWeb = new SendGrid.Web(credentials);
            return transportWeb.DeliverAsync(email);
        }

        private SendGridMessage ConfigEmail(IdentityMessage message)
        {
            var email = new SendGridMessage();
            email.AddTo(message.Destination);
            email.Subject = message.Subject;
            email.Text = message.Body;
            email.Html = message.Body;

            return email;
        }

        public void Dispose()
        {
        }
    }
}