using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class EventParticipant
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int UserId { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public User User { get; set; }
        public DateTime AssignDate { get; set; }
        public EventPaymentStatus Status { get; set; }
        public string PayIntentId { get; set; }
        public string EventLiveToken { get; set; }
    }

    public enum EventPaymentStatus : byte
    {
        NOT_STARTED,
        PENDING,
        SUCCESS,
        DECLINED,
        PAYMENT_CANCELED,
        REFUNDED
    }
}
