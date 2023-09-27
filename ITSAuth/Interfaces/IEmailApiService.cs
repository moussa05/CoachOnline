using ITSAuth.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITSAuth.Interfaces
{
    public interface IEmailApiService
    {
        public void SendEmail(EmailMessage message);
        public Task SendEmailAsync(EmailMessage message);

    }
}
