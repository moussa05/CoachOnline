using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.PayPalIntegration.Model
{
    public class PayPalPayoutRqs
    {
        public PayPalSenderBatch sender_batch_header { get; set; }
        public List<PayPalPayoutItem> items { get; set; }
    }

    public class PayPalPayoutItem
    {
        public string recipient_type { get; set; } = "EMAIL";
        public string receiver { get; set; }
        public PayPalPayoutAmonut amount { get; set; }
        public string sender_item_id { get; set; }
        public string note { get; set; }
        public string notification_language { get; set; } = "fr-FR";
    }

    public class PayPalPayoutAmonut
    {
        public string value { get; set; }
        public string currency { get; set; }
    }
    public class PayPalSenderBatch
    {
        public string sender_batch_id { get; set; }
        public string email_subject { get; set; }
        public string email_message { get; set; }
    }
}
