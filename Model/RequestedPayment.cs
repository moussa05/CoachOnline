using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class RequestedPayment
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PayPalEmail { get; set; }
        public string PayPalPhone { get; set; }
        public string PayPalPayerId { get; set; }
        public string Currency { get; set; }
        public decimal PaymentValue { get; set; }
        public RequestedPaymentStatus Status { get; set; }
        public string RejectReason { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? StatusChangeDate { get; set; }
        public List<AffiliatePayment> Payments { get; set; }

        public List<CoachBalanceDay> CoachPayments { get; set; }
        public RequestedPaymentType PaymentType { get; set; }

    }


    public enum RequestedPaymentType : byte
    {
        Affiliation,
        CoachPayout
    }
    public enum RequestedPaymentStatus : byte
    {
        Prepared,
        Requested,
        Rejected,
        Withdrawn
    }
}
