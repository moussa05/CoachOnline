using CoachOnline.Implementation.Exceptions;
using ITSAuth.Interfaces;
using ITSAuth.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Implementation
{
    public class SendgridImplementation : IEmailApiService
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
            var response = await cli.SendEmailAsync(sendGridMsg);
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new CoachOnlineException($"Can't send email with error {response.StatusCode}.", CoachOnlineExceptionState.Internal);
            }
        }
    }
}
