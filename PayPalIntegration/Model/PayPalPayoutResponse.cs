using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.PayPalIntegration.Model
{
    public class PayPalPayoutResponse
    {
        public PayPalBatchHeader batch_header { get; set; }
        public List<PayPalLinks> links { get; set; }
    }
    public class PayPalBatchHeader
    {
        public string payout_batch_id { get; set; }
        public string batch_status { get; set; }
        public string funding_source { get; set; }
        public PayPalSenderBatch sender_batch_header { get; set; }
        public PayPalPayoutAmonut amount { get; set; }
    }

    public class PayPalSenderBatchHeader
    {
        public string sender_batch_id { get; set; }
        public string email_subject { get; set; }
        public string email_message { get; set; }
    }

    public class PayPalLinks
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
        public string encType { get; set; }
    }
}
