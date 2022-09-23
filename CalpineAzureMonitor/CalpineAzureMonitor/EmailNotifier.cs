using System;
using System.Net.Mail;
using log4net;

namespace CalpineAzureMonitor
{
    public class EmailNotifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailNotifier));

        private readonly string _smtpServer;
        private readonly string _defaultFrom;

        public EmailNotifier(string smtpServer, string defaultFrom = null)
        {
            _smtpServer = smtpServer;
            _defaultFrom = defaultFrom;
        }

        public void SendEmail(string to, string subject, string body)
        {
            SendEmail(to, _defaultFrom, subject, body);
        }

        public void SendEmail(string to, string from, string subject, string body)
        {
            var mailMessage = new MailMessage();
            foreach (var address in to.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)) {
                mailMessage.To.Add(address);
            }
            mailMessage.From = new MailAddress(from);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient(_smtpServer);
            try {
                smtpClient.SendMailAsync(mailMessage).GetAwaiter().GetResult();
            }
            catch (Exception ex) {
                Log.Error("An exception occurred sending email:", ex);
            }
        }
    }
}