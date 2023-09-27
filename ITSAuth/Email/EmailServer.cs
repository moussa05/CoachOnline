using ITSAuth.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using ITSAuth.Interfaces;
using System.Threading.Tasks;

namespace ITSAuth.Implementation.Email
{
    public class EmailServer
    {
        public EmailServer(string Host, string Login, string Password, string EmailAddress, string FriendlyName, EmailServiceAuthorization Authorization, int Port = 80)
        {
            this.Host = Host;
            this.Login = Login;
            this.Password = Password;
            this.EmailAddress = EmailAddress;
            this.Authorization = Authorization;
            this.Port = Port;
            this.FriendlyName = FriendlyName;
        }

        public EmailServer(IEmailApiService apiService)
        {
            this.apiService = apiService;
            this.Authorization = EmailServiceAuthorization.API;
        }

        public int Id { get; set; }
        private string Host { get; }
        private int Port { get; }
        private string Login { get; }
        private string Password { get; }
        private string EmailAddress { get; }
        private string FriendlyName { get; }
        private EmailServiceAuthorization Authorization { get; }
        public enum EmailServiceAuthorization { HTTP, HTTPS, TLS, API }
        IEmailApiService apiService { get; }



        public void SendEmail(EmailMessage message)
        {
            if (Authorization == EmailServiceAuthorization.API)
            {
                throw new NotSupportedException("Instance not configured for API calls, Can't send with this method. Use SendEmailAPI instead.");
            }

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(this.FriendlyName, this.EmailAddress));
            mailMessage.To.Add(new MailboxAddress(message.ReceiverName, message.ReceiverEmail));
            mailMessage.Subject = message.Topic;
            mailMessage.Body = new TextPart("html")
            {
                Text = message.Body
            };

            bool ssl = false;

            switch (this.Authorization)
            {
                case EmailServiceAuthorization.HTTP:
                    ssl = false;
                    break;
                case EmailServiceAuthorization.HTTPS:
                    ssl = true;
                    break;
                case EmailServiceAuthorization.TLS:
                    ssl = true;
                    break;
                default:
                    ssl = false;
                    break;
            }

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(this.Host, this.Port, ssl);
                smtpClient.Authenticate(this.Login, this.Password);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }
        }

        public void SendEmailAPI(EmailMessage message)
        {

            if (Authorization != EmailServiceAuthorization.API)
            {
                throw new NotSupportedException("Instance configured for API calls, Can't send with this method. Use SendEmail instead.");
            }
            apiService.SendEmail(message);

        }

        public async Task SendEmailAPIAsync(EmailMessage message)
        {

            if (Authorization != EmailServiceAuthorization.API)
            {
                throw new NotSupportedException("Instance configured for API calls, Can't send with this method. Use SendEmail instead.");
            }
            await apiService.SendEmailAsync(message);

        }
    }
}
