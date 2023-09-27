using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class RequestedPaymentResponse
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PayPalEmail { get; set; }
        //public string PayPalPhone { get; set; }
        public string PayPalPayerId { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
        public RequestedPaymentStatus Status { get; set; }
        public string StatusStr
        {
            get
            {
                return Status.ToString();
            }
        }

        public RequestedPaymentType PaymentType { get; set; }
        public string PaymentTypeStr
        {
            get
            {
                return PaymentType.ToString();
            }
        }
        public DateTime RequestDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string RejectReason { get; set; }
        public int AffiliatePaymentsTotal { get; set; }
        public List<AffiliaterequestedPaymentsResponse> PaymentsRequests { get; set; }
        public List<CoachRequestedPaymentResponse> CoachRequestedPayments { get; set; }

        public PayoutType PayoutType { get; set; }
        public string PayoutTypeStr { get
            {
                return PayoutType.ToString();
            } }

    }

    public enum PayoutType
    {
        Paypal,
        Stripe
    }

    public class AffiliaterequestedPaymentsResponse
    {
        public int Id { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
        public int AffiliateId { get; set; }
        public DateTime PaymentCreationDate { get; set; }
    }

    public class CoachRequestedPaymentResponse
    {
        public int Id { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
        public DateTime ForDay { get; set; }
    }
}
