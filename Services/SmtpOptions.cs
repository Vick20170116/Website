using System.Net;
using System.Net.Mail;

namespace NaimeiKnowledge.Services
{
    public class SmtpOptions
    {
        public ICredentialsByHost Credentials { get; set; }

        public SmtpDeliveryFormat DeliveryFormat { get; set; }

        public SmtpDeliveryMethod DeliveryMethod { get; set; }

        public bool EnableSsl { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public int Timeout { get; set; }

        public bool UseDefaultCredentials { get; set; }
    }
}
