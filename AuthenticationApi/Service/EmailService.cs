using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AuthenticationApi.Service {
    public class EmailSettings {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "";
    }

    public class EmailService : IEmailSender<IdentityUser> {
        public readonly EmailSettings settings;
        public EmailService(IOptions<EmailSettings> options) {
            settings = options.Value;
        }

        public async Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink) {
            string subject = "Confirm your email";
            string body = $"Please confirm your email by clicking the link: <a href=\"{confirmationLink}\">Confirm Email</a>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode) {
            string subject = "Reset your password";
            string body = $"Reset your password using this code: <b>{resetCode}</b>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink) {
            string subject = "Reset your password";
            string body = $"Reset your password using this link: <a href=\"{resetLink}\">Reset Password</a>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailAsync(IdentityUser user, string subject, string htmlMessage) {
            await SendEmailAsync(user.Email, subject, htmlMessage);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage) {
            using var client = new SmtpClient(settings.Host, settings.Port) {
                EnableSsl = settings.EnableSSL,
                Credentials = new NetworkCredential(settings.UserName, settings.Password)
            };

            var mail = new MailMessage() {
                From = new MailAddress(settings.From),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}
