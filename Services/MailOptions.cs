using System.Net.Mail;

namespace NaimeiKnowledge.Services
{
    public class MailOptions
    {
        public MailAddress From { get; set; }

        public SmtpOptions SmtpOptions { get; } = new SmtpOptions();
    }
}
