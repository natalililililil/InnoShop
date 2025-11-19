using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Users.Application.Services;

namespace Users.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var host = emailSettings["SmtpHost"] ?? "smtp.mailtrap.io";
            var port = int.Parse(emailSettings["SmtpPort"] ?? "2525");

            var senderEmail = emailSettings["SenderEmail"] ??
                              throw new InvalidOperationException("SenderEmail (From address) is not configured.");

            var senderUsername = emailSettings["SenderUsername"] ??
                                 throw new InvalidOperationException("SenderUsername is not configured.");
            var senderPassword = emailSettings["SenderPassword"] ??
                                 throw new InvalidOperationException("SenderPassword is not configured.");

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;

                client.Credentials = new NetworkCredential(senderUsername, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Inno Shop"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}