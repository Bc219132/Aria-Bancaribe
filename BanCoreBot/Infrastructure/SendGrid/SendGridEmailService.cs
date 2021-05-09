using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BanCoreBot.Infrastructure.SendGrid
{
    public class SendGridEmailService : ISendGridEmailService
    {
        IConfiguration _configuration;

        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> Execute(
        string fromEmail,
        string fromName,
        string toEmail,
        string toName,
        string subject,
        string plainTextContent,
        string htmlContent
        )
        {
            
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(toEmail, toName));
            msg.From = new MailAddress(fromEmail, fromName);
            msg.Subject = subject;
            msg.Body = plainTextContent;
            msg.IsBodyHtml = false; 
            var mail = _configuration["correoAria"];
            var pass = _configuration["ClaveAria"];

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(mail, pass);
            client.Port = 587; // You can use Port 25 if 587 is blocked (mine is!)
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


            /*
            
            var apiKey = _configuration["SendGridEmail"];
            var client = new SendGridClient(apiKey);
            var From = new EmailAddress(fromEmail, fromName);
            var To = new EmailAddress(toEmail, toName);

            var email = MailHelper.CreateSingleEmail(From, To, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(email);

            if (response.StatusCode.ToString().ToLower() == "unanthorized") return false;
            return true;
            */

        }
    }
}
