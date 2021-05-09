using System.Threading.Tasks;

namespace BanCoreBot.Infrastructure.SendGrid
{
    public interface ISendGridEmailService
    {
        Task<bool> Execute(
        string fromEmail,
        string fromName,
        string toEmail,
        string toName,
        string subject,
        string plainTextContent,
        string htmlContent
        );
    }
}
