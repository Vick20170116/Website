using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace NaimeiKnowledge.Services
{
    public class MailSender : IMailSender
    {
        public MailSender(IOptions<MailOptions> options)
        {
            this.Options = options.Value;
        }

        public MailOptions Options { get; }

        public Task SendConfirmationMailAsync(string email, string confirmationLink, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = new MailMessage
            {
                From = this.Options.From,
                Subject = "請驗證您的帳號",
                IsBodyHtml = true,
                Body =
                $"如果您沒有在耐美知識網註冊，請忽略此信件。<br />" +
                $"點擊連結以驗證您的耐美知識網帳號：" +
                $"<a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>{HtmlEncoder.Default.Encode(confirmationLink)}</a>",
            };
            message.To.Add(email);
            return this.SendMailAsync(message, cancellationToken);
        }

        public async Task SendMailAsync(MailMessage message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var smtpClient = new SmtpClient
            {
                DeliveryFormat = this.Options.SmtpOptions.DeliveryFormat,
                DeliveryMethod = this.Options.SmtpOptions.DeliveryMethod,
                EnableSsl = this.Options.SmtpOptions.EnableSsl,
                Host = this.Options.SmtpOptions.Host,
                Port = this.Options.SmtpOptions.Port,
                Timeout = this.Options.SmtpOptions.Timeout,
                UseDefaultCredentials = this.Options.SmtpOptions.UseDefaultCredentials
            })
            {
                if (!(this.Options.SmtpOptions.UseDefaultCredentials))
                {
                    smtpClient.Credentials = this.Options.SmtpOptions.Credentials;
                }

                cancellationToken.Register(smtpClient.SendAsyncCancel);
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}
