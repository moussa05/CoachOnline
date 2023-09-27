using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public int CoachId { get; set; }
        public virtual User Coach { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PersonQty { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string CoverPictrueUrl { get; set; }
        public List<EventAttachment> Attachments { get; set; }
        public List<EventCategory> Categories { get; set; }
        public EventStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public List<EventParticipant> Participants { get; set; }
        public List<EventPartner> Partners { get; set; }
        public string EventChannelName { get; set; }
        public string CoachLiveToken { get; set; }

    }

    public class EventPartner
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public string PartnerName { get; set; }
        public string PhotoUrl { get; set; }

        [NotMapped]
        public string PhotoBase64 { get; set; }
    }

    public enum EventStatus : byte
    {
        DRAFT,
        CREATED,
        DELETED,
        FINISHED
    }

    public class EventAttachment
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }

    public class EventCategory
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
