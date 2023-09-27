using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IPaymentService
    {
        Task<string> GenerateConnectedAccount(string AuthToken, string countryCode);
        Task<string> GenerateConnectedAccountAsPrivateUser(string AuthToken, string countryCode);
        Task<string> VerificationFirstStage(string AuthToken);
        Task<string> VerificationKYCStage(string AuthToken);
    }
}
