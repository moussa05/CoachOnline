using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.Coach;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IEvent
    {
        Task<int> CreateEventDraft(int coachId);
        Task<int> UpdateEventDraft(int coachId, int eventId, string name, string description, string base64Pic, DateTime? startDate, DateTime? endDate, List<EventCategoryRqs> categories);
        Task<int> UpdateEventDraft(int coachId, int eventId, int? personQty, decimal? price, string currency, List<EventAttachmentRqs> attachments);
        Task CreateEvent(int eventId, int coachId);
        Task<List<EventInfoResponse>> GetUserEvents(int coachId);
        Task<EventInfoResponse> GetUserEvent(int eventId, int coachId);
        Task UpdateEventField(int eventId, int coachId, string propertyName, object value);
        Task<List<EventInfoResponse>> GetIncomingEvents();
        Task<List<PaymentMethodsResponse>> GetPaymentMethods(int userId);
        Task<int> AssignForEvent(int userId, int eventId);
        Task PayForEvent(int userId, int assignId, string paymentMethodId);
        Task RefundCustomerPaymentForEvent(int userId, int assignationId, bool forEventDeletion = false);
        Task UpdateEventDates(int coachId, int eventId, DateTime startDate, DateTime endDate);
        Task DeleteMyEvent(int coachId, int eventId);
        Task<string> JoinLiveEvent(int userId, int eventId);
        Task<string> StartMyLiveEvent(int eventId, int coachId);
    }
}
