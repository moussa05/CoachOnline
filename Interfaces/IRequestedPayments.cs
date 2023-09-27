using CoachOnline.Model;
using CoachOnline.Model.ApiResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IRequestedPayments
    {
        Task AcceptPayPalWithdrawal(int withdrawalId);
        Task RejectPaypalWithdrawal(int withdrawalId, string reason);
        Task<List<RequestedPaymentResponse>> GetPaypalWithdrawalRequestedPaymentsForUser(int userId);
        Task<List<RequestedPaymentResponse>> GetPaypalWithdrawalRequestedPayments(RequestedPaymentStatus? status);
    }
}
