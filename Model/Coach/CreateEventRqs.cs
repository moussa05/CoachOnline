using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Coach
{
    public class CreateEventRqs
    {
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public DateTime? EventStartDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string CoverPictrueBase64 { get; set; }
        public decimal? TicketPrice { get; set; } = null;
        public string Currency { get; set; }
        public int? ParticipantsQty { get; set; } = null;
        public List<EventCategoryRqs> Categories { get; set; }
        public List<EventAttachmentRqs> Attachments { get; set; }
    }

    public class UpdateEventRqs
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PersonQty { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string CoverPictrueBase64 { get; set; }
        public List<EventAttachmentRqs> Attachments { get; set; }
        public List<EventCategoryRqs> Categories { get; set; }
        public List<EventPartnerRqs> Partners { get; set; }
    }

    public class EventAttachmentRqs
    {
        public string AttachmentBase64 { get; set; }
        public string AttachmentName { get; set; }
    }

    public class EventPartnerRqs
    {
        public string PhotoBase64 { get; set; }
        public string PartnerName { get; set; }
    }

    public class EventCategoryRqs
    {
        public int CategoryId { get; set; }
    }
}
