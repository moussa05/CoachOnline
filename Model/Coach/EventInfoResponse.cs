using CoachOnline.Model.ApiRequests.ApiObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Coach
{
    public class EventInfoResponse
    {
        public int EventId { get; set; }
        public int CoachId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CoverPictrueUrl { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int PersonQty { get; set; }
        public List<EventCategoryResponse> Categories { get; set; }
        public List<EventAttachmentResponse> Attachments { get; set; }
        public List<EventPartnerResponse> Partners { get; set; }
        public EventStatus Status { get; set; }
        public string StatusStr { get; set; }
        public UserAPI Coach { get; set; }
        public int ParticipantAssigned { get; set; }
        public List<UserAPI> EventParticipants {get;set;}
    }

    public class EventCategoryResponse
    {
        public int EventCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ParentName { get; set; }
        public int? ParentId { get; set; }
    }

    public class EventPartnerResponse
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string PhotoUrl { get; set; }
        public string PhotoUrlFull { get { return $"images/{PhotoUrl}"; } }
    }

    public class EventAttachmentResponse
    {
        public int Attachmentid { get; set; }
        public string AttachmentUrl { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentExtension { get; set; }
    }
}
