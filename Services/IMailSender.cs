using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace NaimeiKnowledge.Services
{
    public interface IMailSender
    {
        Task SendConfirmationMailAsync(string email, string confirmationLink, CancellationToken cancellationToken = default);

        Task SendMailAsync(MailMessage message, CancellationToken cancellationToken = default);
    }
}
