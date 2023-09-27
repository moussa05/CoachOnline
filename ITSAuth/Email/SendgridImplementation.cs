using ITSAuth.Interfaces;
using ITSAuth.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITSAuth.Implementation.Email
{
    class SendgridImplementation : IEmailApiService
    {

        SendGridClient cli;
        public SendgridImplementation(string apiKey)
        {
            this.cli = new SendGridClient(apiKey);
        }

        public void SendEmail(EmailMessage message)
        {
            SendGridMessage sendGridMsg = new SendGridMessage();
            sendGridMsg.HtmlContent = message.Body;
            sendGridMsg.AddTo(message.ReceiverEmail, message.ReceiverName);
            sendGridMsg.Subject = message.Topic;
            sendGridMsg.From = new EmailAddress(message.AuthorEmail, message.AuthorName);
            cli.SendEmailAsync(sendGridMsg).Wait();
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            SendGridMessage sendGridMsg = new SendGridMessage();
            sendGridMsg.HtmlContent = message.Body;
            sendGridMsg.AddTo(message.ReceiverEmail, message.ReceiverName);
            sendGridMsg.Subject = message.Topic;
            sendGridMsg.From = new EmailAddress(message.AuthorEmail, message.AuthorName);
            await cli.SendEmailAsync(sendGridMsg);
        }




    }
}
