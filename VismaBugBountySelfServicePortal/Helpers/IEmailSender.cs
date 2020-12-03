using System.Threading.Tasks;

namespace VismaBugBountySelfServicePortal.Helpers
{
    public interface IEmailSender
    {
        void SendEmail(EmailMessage message);
        Task SendEmailAsync(EmailMessage message);
    }
}
