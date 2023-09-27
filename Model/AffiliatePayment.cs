using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class AffiliatePayment
    {
        [Key]
        public int Id { get; set; }
        public int AffiliateId { get; set; }
        public int HostId { get; set; }
        public DateTime PaymentCreationDate { get; set; }
        public string PaymentCurrency { get; set; }
        public decimal PaymentValue { get; set; }
        public DateTime PaymentForMonth { get; set; }
        public bool IsFirstPayment { get; set; }
        public bool Transferred { get; set; }
        public DateTime? TransferDate { get; set; }
        public bool FirstGeneration { get; set; }
        public string PayPalPayoutId { get; set; }
        public bool IsAffiliateCoach { get; set; }
        public AffiliateModelType AffiliateModelType { get; set; }
        public DateTime? NextPlannedPaymentDate { get; set; }
        public bool FullYearPayment { get; set; }
        public int? DirectHostPayIdRef { get; set; }
        public int? RequestedPaymentId { get; set; }
        public RequestedPayment RequestedPayment { get; set; }

    }
}
